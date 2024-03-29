﻿using Serilog.Core;
using SimPrinter.Core.EventArgs;
using SimPrinter.Core.Logging;
using SimPrinter.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimPrinter.Core.ByteParsers
{
    /// <summary>
    /// Esc/Pos 명령어 바이트를 분석해서 텍스트를 추출한다.
    /// </summary>
    public class EscPosByteParser : IByteParser
    {
        private readonly Logger logger = LoggingManager.Logger;

        /// <summary>
        /// 영수증 끝을 식별할 수 있는 명령어. 다음중 하나라도 포함되어 있으면 출력물이 끝났다고 판단한다.
        /// 앞에서부터 순서대로 검색한다.
        /// </summary>
        private readonly PrintCommand[] printoutEndCommand = new PrintCommand[]
        {
             PrintCommand.EndOfPrintout, PrintCommand.PartialCut, PrintCommand.FullCut, PrintCommand.SelectCutModeAndCutPaper
        };

        /// <summary>
        /// 텍스트 인코딩
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        /// 프린터 명령어 제거기
        /// </summary>
        private readonly PrintCommandRemover printCommandRemover;

        /// <summary>
        /// 출력물버퍼
        /// </summary>
        private byte[] printoutBuffer = new byte[1024];

        /// <summary>
        /// 출력물버퍼 복사 위치
        /// </summary>
        private int printoutBufferPostion;

        public event EventHandler<ByteParsingArgs> ParsingCompleted;

        public EscPosByteParser(Encoding encoding)
        {
            this.encoding = encoding;
            this.printCommandRemover = new PrintCommandRemover();
        }

        public void Parse(byte[] buffer, int offset, int length)
        {
            /*
             * 1. 영수증버퍼로 데이터를 옮긴다.
             * 2. 영수증 끝이 발견되면 
             * 2-1. 끝 위치까지 데이터 추출.
             * 2-1. ESC/POS 커맨드 제거.
             * 2-2. 영수증버퍼를 문자열로 변환한다.
             * */
            
            if(printoutBuffer.Length < printoutBufferPostion + length)
                Array.Resize(ref printoutBuffer, printoutBufferPostion + length);

            Array.Copy(buffer, offset, printoutBuffer, printoutBufferPostion, length);
            printoutBufferPostion += length;

            int index = FindEndOfReceipt(out int endOfReceiptLength);
            
            if (index == -1)
                return;

            // 버퍼 데이터 복사
            int rawReceiptLength = index + endOfReceiptLength;
            byte[] rawReceipt = new byte[rawReceiptLength];
            Array.Copy(printoutBuffer, rawReceipt, rawReceiptLength);
            logger.Information("Found End of receipt: {array}", BitConverter.ToString(rawReceipt, 0, rawReceipt.Length));

            // 버퍼 초기화
            printoutBufferPostion -= rawReceiptLength;
            byte[] temp = new byte[printoutBufferPostion];
            Array.Copy(printoutBuffer, temp, temp.Length);
            printoutBuffer = temp;

            // ESC/POS 커맨드 제거.
            byte[] receipt = printCommandRemover.Remove(rawReceipt);
            logger.Information("Removed command: {array}", BitConverter.ToString(receipt, 0, receipt.Length));

            string text = encoding.GetString(receipt);
            ParsingCompleted?.Invoke(this, new ByteParsingArgs(rawReceipt, 0, rawReceiptLength, receipt, 0, receipt.Length, text));
        }

        /// <summary>
        /// 영수증 끝부분 인덱스 검색
        /// </summary>
        /// <param name="endCommandLength">끝부분 길이</param>
        /// <returns></returns>
        private int FindEndOfReceipt(out int endCommandLength)
        {
            foreach(var endCommand in printoutEndCommand)
            {
                int index = ArrayUtil.FindIndex(printoutBuffer, endCommand.Code, 0);
                if (0 < index)
                {
                    endCommandLength = endCommand.TotalLength;
                    return index;
                }
            }
            endCommandLength = 0;
            return -1;
        }

    }
}
