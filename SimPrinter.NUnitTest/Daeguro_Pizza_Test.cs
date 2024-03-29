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
    public class Daeguro_Pizza_Test
    {
        public const string ORDER_TIME = "2021-11-04 11:30:35";
        public const string CONTACT = "050389140859";
        public const string ADDRESS = "대구광역시 수성구 파동 160-11" + "\r\n" + "대구광역시 수성구 파동로 125" + "\r\n" + " 파동부부통닭";
        public const string MEMO1 = "리뷰이벤트 치즈크러스트주세요~^^(수저포크 X)";
        public const string MEMO2 = "조심히 안전운행해서 와주세요";

        private string testText;
        private string[] textLines;

        private string orderText2;
        private string[] orderTextLines2;


        [SetUp]
        public void Setup()
        {
            testText = string.Format(@"
가맹점 명 : 피자알볼로 지산범물점
가맹점 주소
대구 수성구 지산동 1268-15
주문시간 : {0}
==========================================
상품명                    수량        가격
------------------------------------------
쉬림프 & 핫치킨골드피        1       31,000
자 L
 ▶L                                      
 ▶기본                                   
배달팁                               2,000
(포장) 하프앤하프            1       31,000
 ▶콤비네이션 R
 ▶포테이토 R
==========================================
                   할인금액 :        5,000
                   결제금액 :       28,000
------------------------------------------
                   총  금액 :       33,000
------------------------------------------
고객 주소
{1}

고객 연락처: {2}

결제방법 : 선결제

고객요청사항: {3}

배달: {4}
------------------------------------------
", ORDER_TIME, ADDRESS, CONTACT, MEMO1, MEMO2);


            textLines = testText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);


            orderText2 = @"
 가맹점 명 : 피자알볼로 지산범물점
가맹점 주소
대구 수성구 지산동 1268-15
주문시간 : 2021-12-29 11:18:28
==========================================
상품명                    수량        가격
------------------------------------------
팔도피자 L                  1       28,000
 ▶L                                      
배달팁                               1,000
==========================================
                   총  금액 :       29,000
------------------------------------------
                   할인금액 :        3,350
                   결제금액 :       25,650
------------------------------------------
고객 주소
대구광역시 수성구 두산동 28-2
대구광역시 수성구 들안로16길 58
 2학년 1반

고객 연락처: 050389141837

결제방법 : 선결제

고객요청사항: 1:30배달해주세요리뷰콜라부탁드립니다^^

배달: 조심히 안전운행 해서 와주세요
------------------------------------------
";
            orderTextLines2 = testText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }


        [Test]
        public void ParseOrderTimeTest()
        {

            DaeguroTextParser textParser = new DaeguroTextParser();

            string orderTime = textParser.ParseOrderTime(textLines);

            Assert.AreEqual(ORDER_TIME, orderTime);

            Console.WriteLine(orderTime);
        }
    
        [Test]
        public void ParseContactNumber()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            string number = textParser.ParseContact(textLines);

            Assert.AreEqual(CONTACT, number);

            Console.WriteLine(number);
        }

        [Test]
        public void ParseAddress()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            string address = textParser.ParseAddress(textLines);

            Assert.AreEqual(ADDRESS.Replace("\r\n", " "), address);

            Console.WriteLine(address);
        }

        [Test]
        public void ParseMemo1()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            string memo = textParser.ParseMemo1(textLines);

            Assert.AreEqual(MEMO1.Replace("\r\n", " "), memo);

            Console.WriteLine(memo);
        }

        [Test]
        public void ParseMemo2()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            string memo = textParser.ParseMemo2(textLines);

            Assert.AreEqual(MEMO2.Replace("\r\n", " "), memo);

            Console.WriteLine(memo);
        }

        [Test]
        public void ParseProduct()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            ProductModel[] products = textParser.ParseProduct(textLines);

            Assert.NotNull(products);
            Assert.IsTrue(0 < products.Length);
            Assert.AreEqual(2, products.Count(x=> x.Type == ProductType.Pizza));
            foreach (var product in products)
                Console.WriteLine(product);
        }

        [Test]
        public void ParseOrder()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            OrderModel order = textParser.Parse(testText);

            Assert.NotNull(order);
            Assert.AreEqual(ORDER_TIME, order.OrderTime);
            Assert.AreEqual(CONTACT, order.Contact);
            Assert.AreEqual(ADDRESS.Replace("\r\n", " "), order.Address);
            string memo = string.Join(Environment.NewLine, MEMO1.Replace("\r\n", " "), MEMO2.Replace("\r\n", " "));
            Assert.AreEqual(memo, order.Memo);

        }

        [Test]
        public void CheckPizzaCount()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            ProductModel[] products = textParser.ParseProduct(textLines);

            Assert.NotNull(products);
            Assert.IsTrue(0 < products.Length);

            Assert.AreEqual(2, products.Count(x => x.Type == ProductType.Pizza));
        }

        [Test]
        public void ParserOrder2()
        {
            DaeguroTextParser textParser = new DaeguroTextParser();

            OrderModel order = textParser.Parse(orderText2);
            Assert.NotNull(order);
            Assert.IsTrue(0 < order.Products.Count());

        }

    }
}
