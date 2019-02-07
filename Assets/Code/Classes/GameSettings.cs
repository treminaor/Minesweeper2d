using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour {

    public Dropdown gridDropdown;

    public static int gridSize;
	// Use this for initialization
	void Start () {
        int savedValue = getGridSizePrefValue();
        gridDropdown.value = savedValue;

        gridDropdown.onValueChanged.AddListener(delegate {
            OnMyValueChange(gridDropdown);
        });
    }

    public void OnMyValueChange(Dropdown dd)
    {
        if (dd.value == 0)
            gridSize = 5;
        else if (dd.value == 1)
            gridSize = 10;
        else
            gridSize = 25;

        PlayerPrefs.SetInt("gridSize", gridSize);
    }

    public int getGridSizePrefValue()
    {
        int size = PlayerPrefs.GetInt("gridSize");

        if (size == 5)
            return 0;
        else if (size == 10)
            return 1;
        else
            return 2;
    }

    public static int getGridSizePref()
    {
        return PlayerPrefs.GetInt("gridSize");
    }

    void OnDestroy()
    {
        gridDropdown.onValueChanged.RemoveAllListeners();
    }

	// Update is called once per frame
	void Update () {
	
	}
}
