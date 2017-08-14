using UnityEngine;
using System.Collections;

public class PlayController : MonoBehaviour {
	
	void Update ()
	{
	    HandleOnMouseButton0Down();
	}

    RaycastHit hit;
    void HandleOnMouseButton0Down()
    {
        int pos = 0;
        if (Input.GetMouseButtonDown(0))
            pos = -1;
        if (Input.GetMouseButtonDown(1))
            pos = 1;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Ray ray;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, float.MaxValue))
            {
                if (hit.transform.CompareTag("Box"))
                {
                    var box = hit.transform.GetComponent<Box>();
                    var org = MapMapper.ins.GetBoxIndex(box);
                    var targetPosition = new Vector3(org.x + pos,org.y, org.z);
                    if (!MapMapper.ins.IsExist(targetPosition))
                    {
                        iTween.MoveTo(hit.transform.gameObject, iTween.Hash("x", hit.transform.position.x + pos, "easeType", "easeInOutExpo", "time", 0.5f, "oncomplete", "OnMoveEnd", "oncompletetarget", hit.transform.gameObject, "oncompleteparams",targetPosition));
                    }
                }
            }
        }
    }
}
