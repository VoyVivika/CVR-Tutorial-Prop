using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using Abi.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ABI.CCK.Scripts.Runtime
{
    public class CCK_RuntimeVariableStream : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(StreamVars());
        }

        private IEnumerator StreamVars()
        {
            OnGuiUpdater updater = gameObject.GetComponent<OnGuiUpdater>();
            string type = updater.asset.type.ToString();

            using (HttpClient httpclient = new HttpClient())
            {
                HttpResponseMessage response;
                response = httpclient.PostAsync(
                    "https://api.abinteractive.net/1/cck/parameterStream",
                    new StringContent(JsonConvert.SerializeObject(new
                        {
                            ContentType = type, ContentId = updater.asset.objectId,
#if UNITY_EDITOR
                            Username = EditorPrefs.GetString("m_ABI_Username"),
                            AccessKey = EditorPrefs.GetString("m_ABI_Key"),
                            UploadRegion = EditorPrefs.GetInt("ABI_PREF_UPLOAD_REGION").ToString()
#endif 
                        }),
                        Encoding.UTF8, "application/json")
                ).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    BaseResponse<VariableStreamResponse> streamResponse = Abi.Newtonsoft.Json.JsonConvert .DeserializeObject<BaseResponse<VariableStreamResponse>>(result);



                    if (streamResponse == null || streamResponse.Data == null)
                    {
#if UNITY_EDITOR
                        EditorUtility.ClearProgressBar();
                        if (UnityEditor.EditorUtility.DisplayDialog("Alpha Blend Interactive CCK",
                            "Request failed. Unable to connect to the Gateway. The Gateway might be unavailable. Check https://status.abinteractive.net for more info.",
                            "Okay"))
                        {
                            EditorApplication.isPlaying = false;
                        }
#endif
                        yield break;
                    }

                    if (!streamResponse.Data.HasPermission)
                    {
#if UNITY_EDITOR
                        EditorUtility.ClearProgressBar();
                        if (UnityEditor.EditorUtility.DisplayDialog("Alpha Blend Interactive CCK",
                            "Request failed. The provided content ID does not belong to your account.", "Okay"))
                        {
                            EditorApplication.isPlaying = false;
                        }
#endif
                        yield break;
                    }

                    if (streamResponse.Data.IsAtUploadLimit)
                    {
#if UNITY_EDITOR
                        EditorUtility.ClearProgressBar();
                        if (UnityEditor.EditorUtility.DisplayDialog("Alpha Blend Interactive CCK",
                            "Request failed. Your account has reached the upload limit. Please consider buying the Unlocked account.",
                            "Okay"))
                        {
                            EditorApplication.isPlaying = false;
                        }
#endif
                    }

                    if (streamResponse.Data.IsBannedFromUploading)
                    {
#if UNITY_EDITOR
                        EditorUtility.ClearProgressBar();
                        if (UnityEditor.EditorUtility.DisplayDialog("Alpha Blend Interactive CCK",
                            "Request failed. Your upload permissions are suspended. For more information, consult your moderation profile in the ABI community hub.",
                            "Okay"))
                        {
                            EditorApplication.isPlaying = false;
                        }
#endif
                    }

                    updater.UploadLocation = streamResponse.Data.UploadLocation;

                    updater.assetName.text = streamResponse.Data.ObjectName;
                    updater.assetDesc.text = streamResponse.Data.ObjectDescription;

                    updater.LoudAudio.isOn = streamResponse.Data.LoudAudio;
                    updater.LongRangeAudio.isOn = streamResponse.Data.LongRangeAudio;
                    updater.SpawnAudio.isOn = streamResponse.Data.SpawnAudio;
                    updater.ContainsMusic.isOn = streamResponse.Data.ContainsMusic;

                    updater.ScreenEffects.isOn = streamResponse.Data.ScreenFx;
                    updater.FlashingColors.isOn = streamResponse.Data.FlashingColors;
                    updater.FlashingLights.isOn = streamResponse.Data.FlashingLights;
                    updater.ExtremelyBright.isOn = streamResponse.Data.ExtremelyBright;
                    updater.ParticleSystems.isOn = streamResponse.Data.ParticleSystems;

                    updater.Violence.isOn = streamResponse.Data.Violence;
                    updater.Gore.isOn = streamResponse.Data.Gore;
                    updater.Horror.isOn = streamResponse.Data.Horror;
                    updater.Jumpscare.isOn = streamResponse.Data.Jumpscare;

                    updater.ExcessivelySmall.isOn = streamResponse.Data.ExtremelySmall;
                    updater.ExcessivelyHuge.isOn = streamResponse.Data.ExtremelyHuge;

                    updater.Suggestive.isOn = streamResponse.Data.Suggestive;
                    updater.Nudity.isOn = streamResponse.Data.Nudity;
                }
            }
        }
    }

    [Serializable]
    public class VariableStreamResponse
    {
        public bool HasPermission { get; set; }
        public bool IsAtUploadLimit { get; set; }
        public bool IsBannedFromUploading { get; set; }

        public string UploadLocation { get; set; }
        
        public string ObjectName { get; set; }
        public string ObjectDescription { get; set; }
        
        public bool LoudAudio { get; set; }
        public bool LongRangeAudio { get; set; }
        public bool SpawnAudio { get; set; }
        public bool ContainsMusic { get; set; }

        public bool ScreenFx { get; set; }
        public bool FlashingColors { get; set; }
        public bool FlashingLights { get; set; }
        public bool ExtremelyBright { get; set; }
        public bool ParticleSystems { get; set; }

        public bool Violence { get; set; }
        public bool Gore { get; set; }
        public bool Horror { get; set; }
        public bool Jumpscare { get; set; }

        public bool ExtremelySmall { get; set; }
        public bool ExtremelyHuge { get; set; }

        public bool Suggestive { get; set; }
        public bool Nudity { get; set; }
    }
    
    public class BaseResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }

        public BaseResponse(string message = null, T data = default)
        {
            Message = message;
            Data = data;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}