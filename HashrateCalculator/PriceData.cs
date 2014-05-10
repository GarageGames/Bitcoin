using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HashrateCalculator
{
/*
{
    "0":"ALF",
    "symbol":"ALF",
    "1":"Alphacoin",
    "name":"Alphacoin",
    "2":"scrypt",
    "algo":"scrypt",
    "3":"561272",
    "currentBlocks":"561272",
    "4":"1.8706648",
    "difficulty":"1.8706648",
    "5":"50",
    "reward":"50",
    "6":"0.5",
    "minBlockTime":"0.5",
    "7":"106463482",
    "networkhashrate":"106463482",
    "price":"0.000001812",
    "exchange":"Cryptsy",
    "exchange_url":"https:\/\/www.cryptsy.com\/users\/register?refid=213505",
    "ratio":1549.9923140024,
    "adjustedratio":1302.5145495819,
    "avgProfit":"1480.1056852708398",
    "avgHash":"106463482.0000"
 }
 */

    public class PriceData
    {
        public string __invalid_name__0 { get; set; }
        public string symbol { get; set; }
        public string __invalid_name__1 { get; set; }
        public string name { get; set; }
        public string __invalid_name__2 { get; set; }
        public string algo { get; set; }
        public string __invalid_name__3 { get; set; }
        public string currentBlocks { get; set; }
        public string __invalid_name__4 { get; set; }
        public string difficulty { get; set; }
        public string __invalid_name__5 { get; set; }
        public string reward { get; set; }
        public string __invalid_name__6 { get; set; }
        public string minBlockTime { get; set; }
        public string __invalid_name__7 { get; set; }
        public string networkhashrate { get; set; }
        public double price { get; set; }
        public string exchange { get; set; }
        public string exchange_url { get; set; }
        public double ratio { get; set; }
        public double adjustedratio { get; set; }
        public object avgProfit { get; set; }
        public object avgHash { get; set; }

        public bool IsEqual(PriceData other)
        {
            return (symbol == other.symbol) && (name == other.name) && (algo == other.algo) && (currentBlocks == other.currentBlocks) && (difficulty == other.difficulty) && (reward == other.reward) &&
                (minBlockTime == other.minBlockTime) && (networkhashrate == other.networkhashrate) && (price == other.price) && (exchange == other.exchange) && (exchange_url == other.exchange_url);                
        }
    }

    public class CoinPrices
    {
        public List<PriceData> prices { get; set; }
    }
}
