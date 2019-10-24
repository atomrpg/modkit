using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest
{
    public string result = null;
    public string errorCode = null;

    public enum ErrorType
    {
        None,
        NetworkError,
        HttpError,
    }

    public ErrorType errorType = ErrorType.None;

    public bool Success
    {
        get { return errorType == ErrorType.None; }
    }

    public JSon.JNode GetData()
    {
        return JSon.JParser.Parse(result)["data"];
    }

    public IEnumerator Do(string url, params IMultipartFormSection[] data)
    {
        result = null;
        errorCode = null;
        errorType = ErrorType.None;

        List<IMultipartFormSection> postData = new List<IMultipartFormSection>(data);
        UnityWebRequest www = UnityWebRequest.Post(url, postData);
        yield return www.SendWebRequest();

        bool isNetworkError = www.isNetworkError;
        bool isHttpError = www.isHttpError;
        if (isNetworkError || isHttpError)
        {
            errorType = isNetworkError ? ErrorType.NetworkError : ErrorType.HttpError;
            errorCode = www.error;
            Debug.Log(errorType.ToString() + ":" + errorCode);
        }

        result = www.downloadHandler.text;
        Debug.Log(result);
    }
}