using UnityEngine;

public class PlayController : MonoBehaviour {
	
	void Update ()
	{
	    HandleOnMouseButton0Down();
	}

    RaycastHit hit;
    void HandleOnMouseButton0Down()
    {
        int offset_X = 0;
        int offset_Z = 0;

        if (Input.GetKeyDown(KeyCode.A))
            offset_X = -1;
        if (Input.GetKeyDown(KeyCode.D))
            offset_X = 1;
        if (Input.GetKeyDown(KeyCode.W))
            offset_Z = 1;
        if (Input.GetKeyDown(KeyCode.S))
            offset_Z = -1;

        if (offset_X != 0 || offset_Z != 0)
        {
            Ray ray;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, float.MaxValue))
            {
                if (hit.transform.CompareTag("Box"))
                {
                    var box = hit.transform.GetComponent<Box>();
                    var org = MapMapper.ins.GetBoxIndex(box);
                    var targetPosition = new Vector3(org.x + offset_X, org.y, org.z + offset_Z);
                    if (!MapMapper.ins.IsExist(targetPosition))
                    {
                        if (offset_X != 0)
                        {
                            iTween.MoveTo(hit.transform.gameObject, iTween.Hash("x", hit.transform.position.x + offset_X, "easeType", "easeInOutExpo", "time", 0.5f, "oncomplete", "OnMoveEnd", "oncompletetarget", hit.transform.gameObject, "oncompleteparams", targetPosition));
                            return;
                        }
                        if (offset_Z != 0)
                        {
                            iTween.MoveTo(hit.transform.gameObject, iTween.Hash("z", hit.transform.position.z + offset_Z, "easeType", "easeInOutExpo", "time", 0.5f, "oncomplete", "OnMoveEnd", "oncompletetarget", hit.transform.gameObject, "oncompleteparams", targetPosition));
                            return;
                        }
                    }
                }
            }
        }
    }
}
