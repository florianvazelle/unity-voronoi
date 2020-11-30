using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRemovePoints : MonoBehaviour
{
    public GameObject prefab;

	void Update(){
		//Vector3 screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		//Vector3 offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        //Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		//Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;

		if (Input.GetMouseButtonDown(0)) {
			Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            Debug.Log("Pressed primary button.");
        }
        if (Input.GetMouseButtonDown(1)){
            Debug.Log("Pressed secondary button.");
        }
	}
}
