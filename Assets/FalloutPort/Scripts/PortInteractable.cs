using UnityEngine;
namespace FalloutPort {
public class PortInteractable : MonoBehaviour {public string id;public string displayName; public float phase; void Start(){var idle=GetComponent<PortIdle>();if(!idle)idle=gameObject.AddComponent<PortIdle>();idle.seed=Mathf.RoundToInt(phase*1000)+displayName.Length;}}
}
