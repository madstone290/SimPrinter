﻿using SimPrinter.Core.Models;
using SimPrinter.Core.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimPrinter.Core.Logging;
using Serilog.Core;
using SimPrinter.Core.Persistence;
using SimPrinter.Core.TextParsers;

namespace SimPrinter.Core
{
    /// <summary>
    /// 작업스레드.
    /// 이벤트를 처리한다.
    /// </summary>
    public class Worker
    {
        private readonly Logger logger = LoggingManager.Logger;

        /// <summary>
        /// 어플리케이션 연결포트
        /// </summary>
        private readonly SimSerialPort appPort;

        /// <summary>
        /// 프린터 연결포트
        /// </summary>
        private readonly SimSerialPort printerPort;
        
        /// <summary>
        /// 영수증 추출기
        /// </summary>
        private readonly IByteParser byteParser;

        /// <summary>
        /// 주문정보 추출기
        /// </summary>
        private readonly Dictionary<PrintoutType, ITextParser> textParsers = new Dictionary<PrintoutType, ITextParser>();

        /// <summary>
        /// 제품유형 분석기
        /// </summary>
        private readonly ProductDistinguisher productDistinguisher = new ProductDistinguisher();

        /// <summary>
        /// 라벨프린터
        /// </summary>
        private readonly ILabelPrinter labelPrinter;

        /// <summary>
        /// 주문DAO
        /// </summary>
        private readonly OrderDao orderDao;

        /// <summary>
        /// 출력물 구분기
        /// </summary>
        private readonly PrintoutDistinguisher printoutDistinguisher = new PrintoutDistinguisher();

        /// <summary>
        /// 주문목록
        /// </summary>
        private readonly List<OrderModel> orders = new List<OrderModel>();

        /// <summary>
        /// 주문목록
        /// </summary>
        public OrderModel[] Orders => orders.ToArray();

        /// <summary>
        /// 라벨프린터
        /// </summary>
        public ILabelPrinter LabelPrinter => labelPrinter;

        public double LabelWidth 
        { 
            get => labelPrinter.PaperWidth;
            set => labelPrinter.PaperWidth = value; 
        }

        public double LabelHeight
        {
            get => labelPrinter.PaperHeight;
            set => labelPrinter.PaperHeight = value;
        }

        public string[] NoPrintProducts 
        {
            get => labelPrinter.NoPrintProducts;
            set => labelPrinter.NoPrintProducts = value;
        }

        public string[] DaeguroPizzas 
        {
            get => textParsers[PrintoutType.DaeguroOrder].Pizzas;
            set => textParsers[PrintoutType.DaeguroOrder].Pizzas = value;
        }

        public string[] DaeguroSideDishes
        {
            get => textParsers[PrintoutType.DaeguroOrder].SideDishes;
            set => textParsers[PrintoutType.DaeguroOrder].SideDishes = value;
        }

        public string[] ZPosPizzas
        {
            get => textParsers[PrintoutType.ZPosOrder].Pizzas;
            set => textParsers[PrintoutType.ZPosOrder].Pizzas = value;
        }

        public string[] ZPosSideDishes
        {
            get => textParsers[PrintoutType.ZPosOrder].SideDishes;
            set => textParsers[PrintoutType.ZPosOrder].SideDishes = value;
        }


        /// <summary>
        /// 주문정보가 생성되었다.
        /// </summary>
        public event EventHandler<OrderArgs> OrderCreated;

        public Worker(SimSerialPort appPort, SimSerialPort printerPort, IByteParser byteParser, ILabelPrinter labelPrinter, OrderDao orderDao)
        {
            if (appPort == null)
                throw new ArgumentNullException(nameof(appPort));
            if (printerPort == null)
                throw new ArgumentNullException(nameof(printerPort));
            if (byteParser == null)
                throw new ArgumentNullException(nameof(byteParser));
            if (labelPrinter == null)
                throw new ArgumentNullException(nameof(labelPrinter));
            if (orderDao == null)
                throw new ArgumentNullException(nameof(orderDao));

            this.appPort = appPort;
            this.printerPort = printerPort;
            this.byteParser = byteParser;
            this.labelPrinter = labelPrinter;
            this.orderDao = orderDao;

            textParsers.Add(PrintoutType.ZPosOrder, new ZPosTextParser());
            textParsers.Add(PrintoutType.DaeguroOrder, new DaeguroTextParser());

            appPort.DataReceived += AppPort_DataReceived;
            printerPort.DataReceived += PrinterPort_DataReceived;
            byteParser.ParsingCompleted += ByteParser_ParsingCompleted;
        }

        /// <summary>
        /// 설정된 일자기준으로 데이터를 복원한다. 
        /// </summary>
        /// <param name="today"></param>
        public void Restore(DateTime date)
        {
            var dateOrders = orderDao.GetOrders(date);
            
            orders.Clear();
            orders.AddRange(dateOrders);
        }

        /// <summary>
        /// 라벨발행
        /// </summary>
        /// <param name="orderId">주문ID</param>
        public void PrintLabel(Guid orderId)
        {
            OrderModel order = orders.FirstOrDefault(x => x.Id == orderId);

            if (order == null)
            {
                logger.Information("주문검색 실패 {Id}", orderId);
                return;
            }
            
            labelPrinter.Print(order);
        }

        private void ByteParser_ParsingCompleted(object sender, ByteParsingArgs e)
        {
            /*
             * 출력물 분석이 완료되면
             * 1. 주문 출력물이 아니면 종료
             * 2. 주문정보 분석
             * 3. 라벨프린터 출력
             * 4. 주문생성 이벤트
             * */
            logger.Information("ReceiptParsed {NewLine}{Receipt}", Environment.NewLine, e.Text);

            if (string.IsNullOrWhiteSpace(e.Text))
                return;

            PrintoutType printoutType = printoutDistinguisher.Distinguish(e.Text);

            if(!textParsers.TryGetValue(printoutType, out ITextParser textParser))
                return;

            OrderModel order = null;
            try
            {
                order = textParser.Parse(e.Text);
                order.OrderNumber = orderDao.GetOrderNumber(DateTime.Today);
                orderDao.Save(DateTime.Today, order);
                orders.Add(order);

                logger.Information("주문분석 완료 {order}", order);
            }
            catch (Exception ex)
            {
                logger.Error("주문분석 에러", ex);
            }

            if (order == null)
                return;

            try
            {
                labelPrinter.Print(order);
            }
            catch(Exception ex)
            {
                logger.Error("라벨프린터 에러", ex);
            }

            OrderCreated?.Invoke(this, new OrderArgs(order, e.RawBufferHex, e.TextBufferHex, e.Text));
        }

        private void AppPort_DataReceived(object sender, SerialDataArgs e)
        {
            /*
             * 앱포트에서 데이터가 수신되면 
             * 1. 프린터포트로 송신
             * 2. 영수증 바이트 분석
             * */
            logger.Information("AppPort_DataReceived {Data}", BitConverter.ToString(e.Buffer, e.Offset, e.Length));

            try
            {
                printerPort.Send(e.Buffer, e.Offset, e.Length);
            }
            catch (Exception ex)
            {
                logger.Error("프린트포트 송신에러", ex);
            }
         

            try
            {
                byteParser.Parse(e.Buffer, e.Offset, e.Length);
            }
            catch(Exception ex)
            {
                logger.Error("바이트파서 분석에러", ex);
            }
            
        }

        private void PrinterPort_DataReceived(object sender, SerialDataArgs e)
        {
            /*
            * 프린트포트에서 데이터가 수신되면 
            * 1. 앱 포트로 송신
            * */
            logger.Information("AppPort_DataReceived {Data}", BitConverter.ToString(e.Buffer, e.Offset, e.Length));

            appPort.Send(e.Buffer, e.Offset, e.Length);
        }


        /// <summary>
        /// 작업을 시작한다.
        /// </summary>
        public void Run()
        {
            appPort.Run();
            printerPort.Run();
        }

        /// <summary>
        /// 작업을 종료한다.
        /// </summary>
        public void Stop()
        {
            appPort.Stop();
            printerPort.Stop();
        }
    }
}
