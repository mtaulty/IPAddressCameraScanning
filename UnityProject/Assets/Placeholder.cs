using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placeholder : MonoBehaviour
{
  public Transform textMeshObject;

  private void Start()
  {
    this.textMesh = this.textMeshObject.GetComponent<TextMesh>();
    this.OnReset();
  }
  public void OnScan()
  {
    this.textMesh.text = "scanning for 30s";

#if !UNITY_EDITOR
    MediaFrameQrProcessing.Wrappers.IPAddressScanner.ScanFirstCameraForIPAddress(
        result =>
        {
          UnityEngine.WSA.Application.InvokeOnAppThread(() =>
          {
            // result here is a System.Net.IPAddress...
            this.textMesh.text = result?.ToString() ?? "not found";
          }, 
          false);
        },
        TimeSpan.FromSeconds(30));
#endif
  }
  public void OnReset()
  {
    this.textMesh.text = "say scan to start";
  }
  TextMesh textMesh;
}
