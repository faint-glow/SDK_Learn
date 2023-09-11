using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WXUserInfo
{
    public string openid;//用户的标识，对当前开发者账号唯一
    public string nickname;//用户昵称
    public int sex;//用户性别，0为男性，1为女性(也可能是1为男性，2为女性)
    public string province;//用户个人资料填写的省份
    public string city;//用户个人资料填写的城市
    public string country;//用户个人资料填写的国家
    public string headimgurl;//用户头像，最后一个数值代表正方形头像大小（有0，46，64，96，132数值可选，0代表640*640正方形头像）,
    public string[] privilege;//用户特权信息,json数组，如微信沃卡用户为（chinaunicom）
    public string unionid;//用户统一标识，针对一个微信开放平台账号下的应用，同一用户的unionid是唯一的
}
public class WXData
{
    public string access_token;//接口调用凭证
    public string expires_in;//接口调用凭证超时时间，单位（秒）
    public string refresh_token;//用户刷新
    public string openid;//授权用户唯一标识
    public string scope;//用户授权的作用域，使用(,)分隔
}
public class WXLogin : MonoBehaviour
{
    public Button button_Login,button_quit;
    public Image image_head;
    public Text text_userName, text_log;
    public GameObject go_lobby;

    private AndroidJavaClass jc = null;
    private AndroidJavaObject jo = null;

    private string APPID = "wx709390eb635c5a74";
    private string SECRET = "3a380be5f64df7cbe3f6ea4b925b51c1";
    private void Start()
    {
        button_Login.onClick.AddListener(Onbutton_login);
        button_quit.onClick.AddListener(()=> 
        {
            Application.Quit();
        });
        jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
    }
    void Onbutton_login()
    {
        jo.Call("Login");
    }

    //当Onbutton_login方法使用jo.Call调用Android Studio中写的Login方法时，内部会回调下面的方法
    public void WXLoginCallBack(string str)
    {
        if(str != "用户取消" && str != "用户拒绝" && str != "其他错误")
        {
            Debug.Log("微信登录成功,code是：" + str);
            text_log.text += "微信登录成功,code是：" + str + "\r\n";
            StartCoroutine(GetWXData(str));
        }
        else
        {
            Debug.Log("微信登录失败,code是：" + str);
        }
    }

    public IEnumerator GetWXData(string code)
    {
        //string ss = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=APPID&secret=SECRET&code=CODE&grant_type=authorization_code";
        //string ss = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=APPID&secret=SECRET&code=CODE&grant_type=authorization_code";
        string url = "https://weixin.qq.com/sns/oauth2/access_token?appid="+APPID+"&secret="+SECRET+"&code"+code + "&grant_type=authorization_code";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if(request.isDone && request.error == null)
        {
            WXData wxData = JsonUtility.FromJson<WXData>(request.downloadHandler.text);
            text_log.text += "wxData:" + wxData.access_token + "\r\n" + "wxData.openid:" + wxData.openid + "\r\n";
            StartCoroutine(GetWXUserInfo(wxData));
        }
    }
    public IEnumerator GetWXUserInfo(WXData wxData)
    {
        if(wxData != null)
        {
            string url_getuser = "https://api.weixin.qq.com/sns/userinfo?access_token=" + wxData.access_token + "&openid=" + wxData.openid;//ACCESS_TOKEN&openid=OPENID"
            UnityWebRequest request = UnityWebRequest.Get(url_getuser);
            yield return request.SendWebRequest();
            if (request.isDone && request.error == null)
            {
                WXUserInfo wxUserInfo = JsonUtility.FromJson<WXUserInfo>(request.downloadHandler.text);
                text_userName.text = wxUserInfo.nickname;
                text_log.text += "\r\n" + "姓名：" + wxUserInfo.nickname + "性别：" + wxUserInfo.sex.ToString() + "  国家："+ wxUserInfo.country;// + wxUserInfo
                StartCoroutine(GetHeadImage(wxUserInfo));
            }
        }
    }

    public IEnumerator GetHeadImage(WXUserInfo wxUserInfo)
    {
        if(wxUserInfo != null)
        {
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(wxUserInfo.headimgurl))
            {
                yield return req.SendWebRequest();
                if(req.isDone && req.error == null)
                {
                    Texture2D texture2d = (req.downloadHandler as DownloadHandlerTexture).texture;
                    Sprite sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
                    image_head.sprite = sprite;
                    go_lobby.SetActive(true);
                }
                else
                {
                    Debug.Log("下载出错" + req.responseCode + "," + req.error);
                    text_log.text = "\r\n" + "下载出错" + req.responseCode + "," + req.error;
                }
            }
        }
    }
}
