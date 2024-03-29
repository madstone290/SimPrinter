﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimPrinter.Core.Models
{
    /// <summary>
    /// 제품
    /// </summary>
    public class ProductModel
    {
        /// <summary>
        /// 제품명
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 수량
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 금액
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 제품유형
        /// </summary>
        public ProductType Type { get; set; }

        /// <summary>
        /// 세트 구성품
        /// </summary>
        public List<ProductModel> SetItems { get; set; } = new List<ProductModel>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}  {1}  {2}  {3}", Name, Quantity, Price, Type);
            return sb.ToString();
        }

        public string PrintDetail()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}  {1}  {2}  {3}", Name, Quantity, Price, Type);
            foreach(var setItem in SetItems)
            {
                sb.AppendLine();
                sb.AppendFormat("-{0}  {1}  {2}", setItem.Name, setItem.Quantity, setItem.Price);
            }
            return sb.ToString();
        }
    }
}
