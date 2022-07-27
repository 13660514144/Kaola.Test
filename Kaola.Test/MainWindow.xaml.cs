using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kaola.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string BaseUrl;
        private RestClient _client;
        public Dictionary<string, string> BaseHeaders;
        private string BaseImg;
        private int rows = 0;
        private string Dir;
        private DateTime StimeRun;
        public MainWindow()
        {
            InitializeComponent();
            Dir = AppDomain.CurrentDomain.BaseDirectory;
            BaseImg = $"{Dir}/1656319541077.jpg";
            _client = new RestClient();
        }
        /// <summary>
        /// JSON body 参数提交
        /// </summary>
        /// <param name="Dict"></param>il
        /// <returns></returns>
        public async Task RestPostJsonBody(string Dict)
        {
            string Result = string.Empty;
            BaseUrl = URL.Text.ToString().Trim();
            Dictionary<string, object> D = new Dictionary<string, object>()
            {
                {"IdCode","62412c5f83e3ebef97021241"},
                { "Role",""},
                { "DelFlg",1},
                { "GroupFlg",""},
                { "LastId",""},
                { "PageNextOrPre","first"},
                { "WhereCollection",new ArrayList() },
                { "rows",20},
                { "pages",20}
            };
            try
            {
                //var client = new RestClient($"{BaseUrl}");
                var request = new RestRequest(BaseUrl, Method.POST);

                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(JsonConvert.SerializeObject(D));
                var response = _client.ExecuteAsync<dynamic>(request).Result;
                Result = response.Content;
                Dispatcher.Invoke(new Action(() => {
                    if (rows > 100)
                    {
                        rows = 1;
                        stack.Children.Clear();
                    }
                    TextBlock txt = CreateTblock();
                    txt.Text = response.Content;
                    this.stack.Children.Add(txt);
                    EEndtime.Text = DateTime.Now.ToString("HH:mm:ss.ffff");
                    //DateTime t = DateTime.ParseExact(SEndtime.Text, "hh:mm:ss.fff", null);
                    TimeSpan nd = DateTime.Now - StimeRun;
                    Overtime.Text = $"耗时：{nd.TotalMilliseconds}";
                }));
            }
            catch (Exception ex)
            {
                if (rows > 100)
                {
                    rows = 1;
                    stack.Children.Clear();
                }
                Dispatcher.Invoke(() => {
                    TextBlock txt = CreateTblock();
                    txt.Text = ex.Message.ToString();
                    this.stack.Children.Add(txt);
                });
            }

        }
        //
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <typeparam name="T">返回结果类型</typeparam>
        /// <param name="partUrl">上传地址</param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="appendHeaders">增加的表头，在原有的表头上增加</param>
        /// <param name="isHasBaseHeader">是否使用原有表头，一般只有登录不用</param>
        /// <returns>反回数据结果</returns>
        public async Task<RestResponse> ExecutePostFilesAsync(string partUrl, Dictionary<string, string> filePaths, Dictionary<string, object> parameters = null, Dictionary<string, string> appendHeaders = null, bool isHasBaseHeader = true)
        {
            var request = CreateRequest(partUrl, Method.POST, appendHeaders, isHasBaseHeader);

            request.AddHeader("Content-Type", "multipart/form-data");

            //添加文件
            foreach (var item in filePaths)
            {
                request.AddFile(item.Key, item.Value);
            }

            //添加参数
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var item in parameters)
                {
                    request.AddParameter(item.Key, item.Value);
                }
            }

            //发送请求
            var result = await SendAsync(request, ToCommaString(filePaths));
            return result;
        }
        /// <summary>
        /// 创建请求对象
        /// </summary>
        /// <param name="partUrl"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private RestRequest CreateRequest(string partUrl, Method method, Dictionary<string, string> appendHeaders, bool isHasBaseHeader)
        {
            /*var url = (BaseUrl?.TrimEnd('/') + "/" + partUrl.TrimStart('/')).TrimStart('/');
            
            if (url.Length < 6 || url.Substring(0, 4).ToUpper() != "HTTP")
            {
                ///throw CustomException.Run("访问Api地址不正确：url:{0} ", url);
            }*/
            var request = new RestRequest(partUrl, method);
            if (isHasBaseHeader && BaseHeaders != null && BaseHeaders.Count > 0)
            {
                request.AddHeaders(BaseHeaders);
            }
            if (appendHeaders != null && appendHeaders.Count > 0)
            {
                request.AddHeaders(appendHeaders);
            }
            return request;
        }
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="request">请求Request</param> 
        /// <param name="json">Post数据，只用于记录日记</param>
        /// <returns></returns>
        private async Task<RestResponse> SendAsync(RestRequest request, string json = "")
        {
            request.Timeout = 120000;
            var response = await RequestExecuteAsync(request, json);
            //过滤系统错误
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            else
            {
                throw CustomException.Run("网络访问错误：访问地址：{0} 状态码：{1} 错误信息：{2} ", _client.BaseUrl + request.Resource, response.StatusCode, response.ErrorMessage);
            }
        }
        /// <summary>
        /// 列表转换成用逗号分隔的字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public  string ToCommaString( Dictionary<string, string> value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            string result = string.Empty;
            foreach (var item in value)
            {
                result += $"{item.Key}:{item.Value},";
            }
            return TrimEndString(result,",");
        }
        /// <summary>
        /// 从当前字符串对象移除指定的字符的所有尾部匹配项
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public  string TrimEndString( string value, string trimStr)
        {
            if (value == null)
            {
                return string.Empty;
            }
            //是否尾部匹配
            bool isEndsWith = value.EndsWith(trimStr);
            //去掉尾部一个字符串
            if (isEndsWith)
            {
                if (value.Length > trimStr.Length)
                {
                    return value.Substring(0, value.Length - trimStr.Length);
                }
                else
                {
                    return string.Empty;
                }
            }
            return value;
        }
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="json">请求数据，用于打印</param>
        /// <returns>请求结果</returns>
        private async Task<RestResponse> RequestExecuteAsync(RestRequest request, string json = "")
        {
            var url = _client.BaseUrl + request.Resource;
            
            var startTime = DateTimeUtil.UtcNowMillis();

            RestResponse response = await _client.ExecuteAsync(request) as RestResponse;

            var endTime = DateTimeUtil.UtcNowMillis();
            var timeSecondSpan = (endTime - startTime) / 1000M;
            //_logger?.LogInformation($"接收数据：timeSpan:{timeSecondSpan}s url:{url} data:{ response.Content} ");

            return response;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.URL.Text == string.Empty || NUM.Text==string.Empty || NUM1.Text == string.Empty)
                {
                    MessageBox.Show("请输入参数");
                    return;
                }
                int num = Convert.ToInt32(NUM.Text);
                int num1 = Convert.ToInt32(NUM1.Text);
                StimeRun = DateTime.Now;
                this.Dispatcher.Invoke(() => {
                    SEndtime.Text = DateTime.Now.ToString("HH:mm:ss.fff");
                });
                if (URL.Text.ToString().IndexOf("124.223")>-1)
                {
                    for (int x = 0; x < num; x++)
                    {
                        await RestPostJsonBody(string.Empty);
                        Task.Delay(num1);
                    }
                }
                else
                {
                    for (int x = 0; x < num; x++)
                    {
                        await RequestURL();
                        Task.Delay(num1);
                    }
                }

            }
            catch (Exception ex)
            {
                if (rows > 100)
                {
                    rows = 1;
                    stack.Children.Clear();
                }
                Dispatcher.Invoke(() => {
                    TextBlock txt = CreateTblock();
                    txt.Text = ex.Message.ToString();
                    this.stack.Children.Add(txt);
                });
            }

        }
        private async Task RequestURL()
        {
            
            try
            {
                BaseUrl = URL.Text.ToString().Trim();
                var filePaths = new Dictionary<string, string>();
                filePaths.Add("image", BaseImg);

                var response = await ExecutePostFilesAsync(BaseUrl, filePaths);

                //var result = JsonConvert.DeserializeObject<KoalaResponse>(response.Content, JsonUtil.JsonSettings);
                this.Dispatcher.Invoke(() => {
                    if (rows > 100)
                    {
                        rows = 1;
                        stack.Children.Clear();
                    }
                    TextBlock txt = CreateTblock();
                    txt.Text = response.Content;
                    this.stack.Children.Add(txt);
                    EEndtime.Text = DateTime.Now.ToString("HH:mm:ss.fff");

                    TimeSpan nd = DateTime.Now - StimeRun;
                    Overtime.Text=$"耗时：{nd.TotalMilliseconds}";
                });
            }
            catch (Exception ex)
            {
                if (rows > 1000)
                {
                    rows = 1;
                    stack.Children.Clear();
                }
                Dispatcher.Invoke(() => {
                    TextBlock txt = CreateTblock();
                    txt.Text = ex.Message.ToString();
                    this.stack.Children.Add(txt);
                });
            }
        }
        private TextBlock CreateTblock()
        {
            TextBlock txt = new TextBlock()
            {
                FontSize = 16,
                FontFamily = new FontFamily("YaHei"),
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                Margin = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(10),
                TextWrapping = TextWrapping.Wrap
            };
            return txt;
        }
        //
    }
    public class KoalaResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("data")]
        public KoalaPerson Data { get; set; }

        [JsonProperty("page")]
        public object Page { get; set; }
    }
    public class KoalaPerson
    {
        [JsonProperty("person_id")]
        public long PersonId { get; set; }
    }
}
