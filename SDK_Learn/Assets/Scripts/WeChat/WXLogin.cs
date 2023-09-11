using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WXUserInfo
{
    public string openid;//�û��ı�ʶ���Ե�ǰ�������˺�Ψһ
    public string nickname;//�û��ǳ�
    public int sex;//�û��Ա�0Ϊ���ԣ�1ΪŮ��(Ҳ������1Ϊ���ԣ�2ΪŮ��)
    public string province;//�û�����������д��ʡ��
    public string city;//�û�����������д�ĳ���
    public string country;//�û�����������д�Ĺ���
    public string headimgurl;//�û�ͷ�����һ����ֵ����������ͷ���С����0��46��64��96��132��ֵ��ѡ��0����640*640������ͷ��,
    public string[] privilege;//�û���Ȩ��Ϣ,json���飬��΢���ֿ��û�Ϊ��chinaunicom��
    public string unionid;//�û�ͳһ��ʶ�����һ��΢�ſ���ƽ̨�˺��µ�Ӧ�ã�ͬһ�û���unionid��Ψһ��
}
public class WXData
{
    public string access_token;//�ӿڵ���ƾ֤
    public string expires_in;//�ӿڵ���ƾ֤��ʱʱ�䣬��λ���룩
    public string refresh_token;//�û�ˢ��
    public string openid;//��Ȩ�û�Ψһ��ʶ
    public string scope;//�û���Ȩ��������ʹ��(,)�ָ�
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

    //��Onbutton_login����ʹ��jo.Call����Android Studio��д��Login����ʱ���ڲ���ص�����ķ���
    public void WXLoginCallBack(string str)
    {
        if(str != "�û�ȡ��" && str != "�û��ܾ�" && str != "��������")
        {
            Debug.Log("΢�ŵ�¼�ɹ�,code�ǣ�" + str);
            text_log.text += "΢�ŵ�¼�ɹ�,code�ǣ�" + str + "\r\n";
            StartCoroutine(GetWXData(str));
        }
        else
        {
            Debug.Log("΢�ŵ�¼ʧ��,code�ǣ�" + str);
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
                text_log.text += "\r\n" + "������" + wxUserInfo.nickname + "�Ա�" + wxUserInfo.sex.ToString() + "  ���ң�"+ wxUserInfo.country;// + wxUserInfo
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
                    Debug.Log("���س���" + req.responseCode + "," + req.error);
                    text_log.text = "\r\n" + "���س���" + req.responseCode + "," + req.error;
                }
            }
        }
    }
}
