﻿using NUnit.Framework;
using SimPrinter.Core;
using SimPrinter.Core.Models;
using SimPrinter.Core.TextParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimPrinter.NUnitTest
{
    /// <summary>
    /// Zpos 피자 기본주문 테스트
    /// </summary>
    public class ZPos_Pizza_Test
    {
        public const string ORDER_TIME = "2021-10-05 22:35:42 (화)";
        public const string SUB_TOTAL = "121,500";
        public const string TOTAL = "121,500";
        public const string CREDIT_AMOUNT = "121,500";
        public const string BILL_AMOUNT = "121,500";
        public const string NUMBER = "053-443-3434";
        public const string ADDRESS = "수성구 지산동  청수로 23길 아테네 203호" + "\r\n" + "문2023 -23 4412";
        public const string MEMO = "주문요청사항1 안녕하세요"  + "\r\n" + "맛있게해주세요!" + "\r\n" + "빨리 갖다주세요!";

        private string testText;

        private string[] textLines;

        private string orderText2;

        [SetUp]
        public void Setup()
        {
            testText = string.Format(@"
PIZZA ALVOLO
                                          
상    호: 피자알볼로 지산범물점
주    소: 대구광역시 수성구 지산동 1268-15 1층
사업자NO: 5026357815
대 표 자: 오자희
전화번호: 0537838495
                                          
POS 번호: 0001
주문번호: 00031
접수유형: PD(매장)
거래일시: {0}
------------------------------------------
     상 품 명              수량    금 액
------------------------------------------
쉬림프n핫치킨골드피자 세트 L
                       1     53,500
  -쉬림프n핫치킨 골드 L                   
  -콜라 1.25L                             
  -치즈오븐스파게티                       
  -콘치즈그라탕 L                         
  -슈크림치즈스틱                         
쉬림프n핫치킨 슈크림무스 L    1     31,000
콜라 1.25L              1      2,000
쉬림프n핫치킨 피자 홀세트 R   1     35,000
  -쉬림프n핫치킨 골드 R                   
  -치즈오븐스파게티                       
  -코카콜라 (홀) 355ml                    
  -코카콜라 (홀) 355ml                    
(포장) 하프앤하프            1       31,000
  -콤비네이션 R
  -포테이토 R
------------------------------------------
소  계        {1}
------------------------------------------
합  계        {2}
                                          
신용카드                           {3}
------------------------------------------
청구 금액                          {4}
                                          
                                          
                                          
수신가능: 010-3234-3345
고객번호: {5}
고 객 명: 고객님(신규)
고객주소: {6}
주문메모: {7}
", ORDER_TIME, SUB_TOTAL, TOTAL, CREDIT_AMOUNT, BILL_AMOUNT, NUMBER, ADDRESS, MEMO);


            textLines = testText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);


            orderText2 = @"
PIZZA ALVOLO
                                          
상    호: 피자알볼로 지산범물점
주    소: 대구광역시 수성구 지산동 1268-15 1층
사업자NO: 5026357815
대 표 자: 오자희
전화번호: 0537838495
                                          
접수유형: PC(매장)
거래일시: 2021-12-30 15:35:39 (목)
------------------------------------------
     상 품 명              수량    금 액
------------------------------------------
(포장)하프세트 정통 오리지널 L
                              1     67,500
  -치즈 오리지널  L                       
    -양파 (+)                 1        750
    -버섯 (+)                 1        750
    -웨지감자 (+)             1        750
  -페페로니 오리지널  L                   
    -양파 (+)                 1        750
    -버섯 (+)                 1        750
    -웨지감자 (+)             1        750
  -콜라 1.25L                             
  -치즈오븐스파게티                       
  -치킨텐더 6조각                         
  -감바스                                 
  -후레쉬모짜렐라샐러드                   
  -콘치즈그라탕 L                         
  -팥치즈스틱        
(포장)쉬림프n핫치킨 슈크림무스 L
                              1     31,000
(포장)콜라 500ml              1      1,500
(포장)콜라 500ml              1      1,500
(포장)콜라 500ml              1      1,500
(포장)콜라 500ml              1      1,500
(포장)콜라 500ml              1      1,500
(포장)콜라 500ml              1      1,500
(포장)콜라 500ml              1      1,500

------------------------------------------
소  계        113,500
------------------------------------------
합  계        113,500
                                          
                                          
                                          
                                          
                                          
------------------------------------------
                                          
▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
Alvolo App 이벤트쿠폰
▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                                          
▶수제로. 제대로. 알볼로.                 
`App->이벤트`아래의 문구를 입력하세요!    
지산범물점           
▶20,000원 이상 주문시 사용가능한         
▶`치즈오픈스파게티` 증정쿠폰을 드려요!   
";
        }


        [Test]
        public void ParseOrderTimeTest()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string orderTime = textParser.ParseOrderTime(textLines);

            Assert.AreEqual(ORDER_TIME, orderTime);

            Console.WriteLine(orderTime);
        }

        [Test]
        public void ParseProductSubTotal()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string subTotal = textParser.ParseSubTotal(textLines);

            Assert.AreEqual(SUB_TOTAL, subTotal);

            Console.WriteLine(subTotal);
        }

        [Test]
        public void ParseProductTotal()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string total = textParser.ParseTotal(textLines);

            Assert.AreEqual(TOTAL, total);

            Console.WriteLine(total);
        }

        [Test]
        public void ParseCreditAmount()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string amount = textParser.ParseCreditAmount(textLines);

            Assert.AreEqual(CREDIT_AMOUNT, amount);

            Console.WriteLine(amount);
        }

        [Test]
        public void ParseBillAmount()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string amount = textParser.ParseBillAmount(textLines);

            Assert.AreEqual(BILL_AMOUNT, amount);

            Console.WriteLine(amount);
        }


        [Test]
        public void ParseContactNumber()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string number = textParser.ParseContact(textLines);

            Assert.AreEqual(NUMBER, number);

            Console.WriteLine(number);
        }

        [Test]
        public void ParseAddress()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string address = textParser.ParseAddress(textLines);

            Assert.AreEqual(ADDRESS.Replace("\r\n", " "), address);

            Console.WriteLine(address);
        }

        [Test]
        public void ParseMemo()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            string memo = textParser.ParseMemo(textLines);

            Assert.AreEqual(MEMO.Replace("\r\n", " "), memo);

            Console.WriteLine(memo);
        }

        [Test]
        public void ParseProduct()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            ProductModel[] products = textParser.ParseProduct(textLines);

            Assert.NotNull(products);
            Assert.IsTrue(0 < products.Length);
            Assert.AreEqual(4, products.Count(x=> x.Type == ProductType.Pizza));
            foreach (var product in products)
                Console.WriteLine(product);
        }

        [Test]
        public void ParseOrder()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            OrderModel order = textParser.Parse(testText);

            Assert.NotNull(order);
            Assert.AreEqual(ORDER_TIME, order.OrderTime);
            Assert.AreEqual(SUB_TOTAL, order.SubTotal);
            Assert.AreEqual(TOTAL, order.Total);
            Assert.AreEqual(CREDIT_AMOUNT, order.CreditAmount);
            Assert.AreEqual(BILL_AMOUNT, order.BillAmount);
            Assert.AreEqual(NUMBER, order.Contact);
            Assert.AreEqual(ADDRESS.Replace("\r\n", " "), order.Address);
            Assert.AreEqual(MEMO.Replace("\r\n", " "), order.Memo);

        }

        [Test]
        public void CheckPizzaCount()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            ProductModel[] products = textParser.ParseProduct(textLines);

            Assert.NotNull(products);
            Assert.IsTrue(0 < products.Length);

            // 쉬림프n핫치킨골드피자 세트 L, 쉬림프n핫치킨 슈크림무스 L, 쉬림프n핫치킨 피자 홀세트 R, 하프앤하프
            Assert.AreEqual(4, products.Count(x => x.Type == ProductType.Pizza));
        }



        [Test]
        public void ParseOrder2()
        {
            ZPosTextParser textParser = new ZPosTextParser();

            OrderModel order = textParser.Parse(orderText2);

            Assert.NotNull(order);
            Assert.AreEqual(15, order.Products[0].SetItems.Count);

        }

    }
}
