using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using OldMoatGames;

public class Controller : MonoBehaviour
{
    #region Variable

    [Header("Imagens")]
    [SerializeField] Animator animImagem = default;
    [SerializeField] Animator animTexto = default;
    [SerializeField] Sprite mesaLogo = default;

    int index;
    [SerializeField] Image img = default;
    RectTransform rct1;
    List<CustomImage> Imagens = new List<CustomImage>();
    int loadImg;
    bool loaded = false;
    [SerializeField] AnimatedGifPlayer gifPlayer = default;

    [Space(5f)]
    [Header("Noticias")]
    [SerializeField] Text noticia = default;
    RectTransform noticiaRect;
    string[] noticias;
    public GameObject noticiaCaixa;

    public static Controller self;

    bool isHidden = true;

    [Space(5f)]
    [Header("Network")]
    public InboxEndpoint InboxEndpoint;

    #endregion

    struct CustomImage
    {
        public Sprite sprite;
        public bool gif;
        public string gifPath;
    }

    private void Awake()
    {
        self = this;
        noticiaRect = noticia.transform.GetComponent<RectTransform>();
        rct1 = img.transform.GetComponent<RectTransform>();
    }

    // Use this for initialization
    void Start()
    {
        //Network
        InboxEndpoint.postRequestHandler.AddListener((_, msg) => OnMessage(msg));

        //Load Images
        string fileAdress = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/Mesa Atual/Imagens";
        DirectoryInfo dir = new DirectoryInfo(fileAdress);
        FileInfo[] info = dir.GetFiles("*.*");
        LoadImg(info);

        //Gif
        gifPlayer.Path = GifPath.CustomPath;

        //Load and fix Texts
        LoadNewsFile();
        UpdateNewsData();
    }

    //Receive messeges from web
    private void OnMessage(string msg)
    {
        if (msg.StartsWith("news"))
        {
            switch(msg.Split('\n')[1])
            {
                case "hide":
                    HideNews();
                    break;
                case "show":
                    ShowNews();
                    break;
                case "prev":
                    CallPrevNews();
                    break;
                case "next":
                    CallNextNews();
                    break;
            }
        }
        else if(msg.StartsWith("audio"))
        {
            LoadAndPlayAudio(msg.Split('\n')[1]);
        }
    }

    //Load and create audio object
    void LoadAndPlayAudio(string audioName)
    {
        GameObject go = new GameObject(audioName);
        AudioSource aus = go.AddComponent<AudioSource>();
        AudioClip ac = Resources.Load(audioName) as AudioClip;
        if(ac != null)
        {
            aus.clip = ac;
            aus.Play();
            Destroy(go, ac.length);
        }
        else
        {
            Destroy(go);
        }
    }

    //Load Texts
    public void LoadNewsFile()
    {
        string fileAdress = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/Mesa Atual/Noticias.txt";
        string[] st = File.ReadAllLines(fileAdress);

        int num = st.Length;
        noticias = new string[num];
        for (int i = 0; i < num; i++)
        {
            string str = st[i];
            noticias[i] = str;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!loaded && loadImg == 0)
        {
            noticia.text = noticias[index];
            loaded = true;
        }

        #region Inputs
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HideNews();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowNews();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CallPrevNews();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CallNextNews();
        }
        #endregion
    }


    public void HideNews()
    {
        img.sprite = mesaLogo;
        animTexto.gameObject.SetActive(false);
        isHidden = true;
        
    }

    //Show news and play animation
    public void ShowNews()
    {
        if (!Imagens[index].gif)
        {
            img.sprite = Imagens[index].sprite;
            gifPlayer.Pause();
        }
        else
        {
            gifPlayer.customPath = Imagens[index].gifPath;
            gifPlayer.Init();
        }
        
        animTexto.gameObject.SetActive(true);
        animImagem.SetTrigger("Play");
        animTexto.SetTrigger("Play");
        StartCoroutine(WaitToShowText());
        isHidden = false;
    }


    public void CallPrevNews()
    {
        index--;
        rct1.localScale = new Vector3(1, 1, 1);
        if (index < 0)
        {
            index = Imagens.Count - 1;
        }

        if (!isHidden)
        {
            animImagem.ResetTrigger("Play");
            animImagem.SetTrigger("Play");
            animTexto.ResetTrigger("Play");
            animTexto.SetTrigger("Play");

            if(!Imagens[index].gif)
            {
                img.sprite = Imagens[index].sprite;
                gifPlayer.Pause();
            }
            else
            {
                gifPlayer.customPath = Imagens[index].gifPath;
                gifPlayer.Init();
            }
            
            StartCoroutine(WaitToShowText());
        }
        UpdateNewsData();
    }


    public void CallNextNews()
    {
        index++;
        rct1.localScale = new Vector3(1, 1, 1);
        if (index > Imagens.Count - 1)
        {
            index = 0;
        }

        if (!isHidden)
        {
            animImagem.SetTrigger("Play");
            animTexto.SetTrigger("Play");

            if (!Imagens[index].gif)
            {
                img.sprite = Imagens[index].sprite;
                gifPlayer.Pause();
            }
            else
            {
                gifPlayer.customPath = Imagens[index].gifPath;
                gifPlayer.Init();
            }

            StartCoroutine(WaitToShowText());
        }
        UpdateNewsData();
    }

    //Wait to activate text animation
    IEnumerator WaitToShowText()
    {
        yield return new WaitForSeconds(0.5f);
        noticiaCaixa.transform.GetComponent<ShowText>().RevealText(noticias[index]);
    }


    //Update the file for web
    void UpdateNewsData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "www", "newsData.txt");
        var myFile = File.Create(path);
        myFile.Close();
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(noticias[index]);
        int nextIndex = index + 1;
        if (nextIndex > Imagens.Count - 1)
        {
            nextIndex = 0;
        }
        writer.WriteLine(noticias[nextIndex]);
        writer.Close();
    }

    //Load all Images
    void LoadImg(FileInfo[] info)
    {
        foreach (FileInfo f in info)
        {
            Texture2D tex = null;
            byte[] fileData;
            if (File.Exists(f.FullName))
            {
                loadImg++;
                fileData = File.ReadAllBytes(f.FullName);
                if(f.Extension == ".gif")
                {
                    Imagens.Add(new CustomImage { gif = true, gifPath = f.FullName });
                }
                else
                {
                    tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                    tex.LoadImage(fileData);
                    Imagens.Add(new CustomImage { sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f), gif = false });
                }
                
                loadImg--;
            }
        }
    }
}