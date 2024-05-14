using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetaDotaServer.Entity
{
    //比赛请求状态
    public enum MatchRequestState
    { 
        None = 0,//未请求
        Waiting = 1,//已请求，排队生成中
        Generating = 2,//生成中
        Success = 3,//成功生成
        Fail = 4,//失败生成
    }
    [Table("User")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        //视频地址
        public string VideoUrl { get; set; }

        //比赛请求字段
        public string RequestMatch { get; set; }

        //请求时间
        public int RequestTime { get; set; }

        //请求状态
        public MatchRequestState MatchRequestState { get; set; }

        //请求失败信息
        public string ErrorMessage { get; set; }

        //生成的视频地址
        public string GenerateUrl { get; set; }

        public bool Request(string match)
        {
            RequestMatch = match;
            return true;
        }

        public bool Pay()
        {
            if (MatchRequestState != MatchRequestState.Success || string.IsNullOrEmpty(GenerateUrl))
                return false;

            VideoUrl = GenerateUrl;
            return true;
        }

    }
}
