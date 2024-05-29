using MetaDotaServer.Controllers;
using MetaDotaServer.Tool;
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
        [Key]
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

        public string Jwt { get; set; }

        public bool Request(string match)
        {
            if (MDSCommonTool.CheckMatchValid(match) && ( MatchRequestState == MatchRequestState.None || MatchRequestState == MatchRequestState.Waiting || MatchRequestState == MatchRequestState.Fail))
            {
                RequestMatch = match;
                // 当前时间
                DateTime now = DateTime.UtcNow;
                // 当前时间与Unix纪元之间的时间差
                TimeSpan timeSpan = now - MDSCommonTool.UnixStartTime;
                // 将时间差转换为秒数
                RequestTime = (int)timeSpan.TotalSeconds;

                MatchRequestState = MatchRequestState.Waiting;

                MDSDbContextFactory.PutMatchRequest(this);

                return true;
            }
            return false;

        }

        public bool Pay()
        {
            if (MatchRequestState != MatchRequestState.Success || string.IsNullOrEmpty(GenerateUrl))
                return false;

            MatchRequestState = MatchRequestState.None;
            RequestMatch = "";
            VideoUrl = GenerateUrl;
            return true;
        }

        public bool StartGenerate()
        {
            MatchRequestState = MatchRequestState.Generating;
            return true;
        }

        public void GenerateSuccess(string url)
        { 
            if (!string.IsNullOrEmpty(url)) {
                MatchRequestState = MatchRequestState.Success;
                GenerateUrl = url;
            }
        }

        public void GenerateFail(string message)
        {
            MatchRequestState = MatchRequestState.Fail;
            ErrorMessage = message;
        }

        public void UpdateJwt(string jwt)
        {
            Jwt = jwt;
        }
    }
}
