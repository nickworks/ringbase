using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Modal : MonoBehaviour {
    public readonly UnityEvent onCancel = new UnityEvent();
    public readonly UnityEvent onSubmit = new UnityEvent();

    public void Cancel(){
        onCancel.Invoke();
        Dispose();
    }
    public void Confirm(){
        onSubmit.Invoke();
        Dispose();
    }
    private void Dispose(){
        onCancel.RemoveAllListeners();
        onSubmit.RemoveAllListeners();
        Destroy(gameObject);
    }
}