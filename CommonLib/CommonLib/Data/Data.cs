using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 資料集合
/// </summary>
namespace CommonLib.Data
{
    /// <summary>
    /// 荷官資料組
    /// </summary>
    public struct DealerData
    {
        public int dealer_id { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string descript { get; set; }
        public string sex { get; set; }
        public string bwh { get; set; }
        public string role { get; set; }
        public string avatar_url { get; set; }
    }

    /// <summary>
    /// 掃描器資料
    /// </summary>
    public class Scanner
    {
        public string Identity { get; set; }    //banker or player
        public string name { get; set; }    //device name
        public string messgae { get; set; } // 記錄Scan的字串
        public DateTime lastScanTime { get; set; }

        public void reset()
        {
            messgae = "";
        }

        public static implicit operator string(Scanner scan)
        {
            return scan.messgae;
        } 
    }

    public struct EmergencyItem
    {
        public int code { get; set; }
        public string reason { get; set; }
        public string data { get; set; } // Json
        [JsonIgnore]
        public bool notify { get; set; }
        [JsonIgnore]
        public bool lockUI { get; set; }
        [JsonIgnore]
        public int level { get; set; }
    }

    public struct ChatData
    {
        public int code { get; set; }
        public string req_key { get; set; }
        public string account { get; set; }
        public string nickname { get; set; }
        public string msg { get; set; }
        public long time { get; set; }
        public int errCode { get; set; }
    }

    /// <summary>
    /// 廳資料
    /// </summary>
    public struct LobbyData
    {
        public string id { get; set; }
        public string zhtw { get; set; }
        public string zhcn { get; set; }
        public string enus { get; set; }
    }

    /// <summary>
    /// 桌資料
    /// </summary>
    public struct TableData
    {
        public string id { get; set; }
        public string zhtw { get; set; }
        public string zhcn { get; set; }
        public string enus { get; set; }
    }

    /// <summary>
    /// 桌詳細資料組
    /// </summary>
    public class DetailTableData1
    {
        [JsonIgnore]
        public string current_game_id { get; set; }
        public string lobby_id { get; set; }
        public string table_id { get; set; }
        public string sn { get; set; }
        public string enus_name { get; set; }
        public string zhcn_name { get; set; }
        public string zhtw_name { get; set; }
        public string enus_desc { get; set; }
        public string zhcn_desc { get; set; }
        public string zhtw_desc { get; set; }
        public int game_time { get; set; }
        public string min_chip { get; set; }
        public string max_chip { get; set; }
        public int dealer_id { get; set; }
        public string host_id { get; set; }
        public string status { get; set; }
        [JsonIgnore]
        public string active { get; set; }
        [JsonIgnore]
        public int live_state { get; set; }
    }

    /// <summary>
    /// 卡牌資料組
    /// </summary>
    public class CardDetailData : DetailTableData1
    {
        public string boots { get; set; }
    }

    /// <summary>
    /// 輪盤資料組
    /// </summary>
    public class RouletteDetailData : DetailTableData1
    {
        public string small_rate { get; set; }
        public string max_small_chip { get; set; }
        public string big_rate { get; set; }
        public string max_big_chip { get; set; }
        public string odd_rate { get; set; }
        public string max_odd_chip { get; set; }
        public string even_rate { get; set; }
        public string max_even_chip { get; set; }
        public string red_rate { get; set; }
        public string max_red_chip { get; set; }
        public string black_rate { get; set; }
        public string max_black_chip { get; set; }
        public string dozen_rate { get; set; }
        public string max_dozen_chip { get; set; }
        public string column_rate { get; set; }
        public string max_column_chip { get; set; }
        public string straight_rate { get; set; }
        public string max_straight_chip { get; set; }
        public string split_rate { get; set; }
        public string max_split_chip { get; set; }
        public string corner_rate { get; set; }
        public string max_corner_chip { get; set; }
        public string street_rate { get; set; }
        public string max_street_chip { get; set; }
        public string line_rate { get; set; }
        public string max_line_chip { get; set; }
    }

    /// <summary>
    /// 骰寶資料組
    /// </summary>
    public class SicboDetailData : DetailTableData1
    {
        public string small_rate { get; set; }
        public string max_small_chip { get; set; }
        public string big_rate { get; set; }
        public string max_big_chip { get; set; }
        public string odd_rate { get; set; }
        public string max_odd_chip { get; set; }
        public string even_rate { get; set; }
        public string max_even_chip { get; set; }
        public string red_rate { get; set; }
        public string max_red_chip { get; set; }
        public string black_rate { get; set; }
        public string max_black_chip { get; set; }
        public string dozen_rate { get; set; }
        public string max_dozen_chip { get; set; }
        public string column_rate { get; set; }
        public string max_column_chip { get; set; }
        public string straight_rate { get; set; }
        public string max_straight_chip { get; set; }
        public string split_rate { get; set; }
        public string max_split_chip { get; set; }
        public string corner_rate { get; set; }
        public string max_corner_chip { get; set; }
        public string street_rate { get; set; }
        public string max_street_chip { get; set; }
        public string line_rate { get; set; }
        public string max_line_chip { get; set; }
    }

    /// <summary>
    /// 百家樂資料組
    /// </summary>
    public class BaccaratDetailData : CardDetailData
    {
        public string squint_time { get; set; }
        public string bkr_rate { get; set; }
        public string max_bkr_chip { get; set; }
        public string plr_rate { get; set; }
        public string max_plr_chip { get; set; }
        public string tie_rate { get; set; }
        public string max_tie_chip { get; set; }
        public string bkr_pair_rate { get; set; }
        public string max_bkr_pair_chip { get; set; }
        public string plr_pair_rate { get; set; }
        public string max_plr_pair_chip { get; set; }
        public string big_rate { get; set; }
        public string max_big_chip { get; set; }
        public string small_rate { get; set; }
        public string max_small_chip { get; set; }
        public string bkr_no_comm_rate { get; set; }
        public string max_bkr_no_comm_chip { get; set; }
    }

    public class DragonBonusBaccaratDetailData : BaccaratDetailData
    {
        public string bkr_dg_rate { get; set; }
        public string max_bkr_dg_chip { get; set; }
        [JsonIgnore]
        public string bkr_dg_count { get; set; }
        [JsonIgnore]
        public string bkr_dg_amount { get; set; }
        public string plr_dg_rate { get; set; }
        public string max_plr_dg_chip { get; set; }
        [JsonIgnore]
        public string plr_dg_count { get; set; }
        [JsonIgnore]
        public string plr_dg_amount { get; set; }
    }

    /// <summary>
    /// 龍虎資料組
    /// </summary>
    public class DragonTigerDetailData : CardDetailData
    {
        public string dragon_rate { get; set; }
        public string max_dragon_chip { get; set; }
        public string tiger_rate { get; set; }
        public string max_tiger_chip { get; set; }
        public string tie_rate { get; set; }
        public string max_tie_chip { get; set; }
    }

    /// <summary>
    /// 歐式輪盤資料組
    /// </summary>
    public class EuropeanRouletteDetailData : RouletteDetailData
    {
        //
    }

    /// <summary>
    /// 美式輪盤資料組
    /// </summary>
    public class AmericanRouletteDetailData : RouletteDetailData
    {
        public string five_rate { get; set; }
        public string max_five_chip { get; set; }
        public string five_count { get; set; }
        public string five_amount { get; set; }
    }

}
