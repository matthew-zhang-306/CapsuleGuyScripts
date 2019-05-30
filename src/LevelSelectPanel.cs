using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelSelectPanel : MenuPanel
{
    // Really this should be in an XML but it's not too large so it's easier to have it here
    static readonly Dictionary<string, Color> groupColors = new Dictionary<string, Color>() {
        ["Opening"] = new Color(0.5f, 0.5f, 1f),
        ["Laser"] = new Color(0.7f, 0.4f, 1f),
        ["LaserHard"] = new Color(1f, 0.5f, 0.9f),
        ["Climax"] = new Color(1f, 0.5f, 0.5f)
    };

    public GameObject pages;
    public GameObject gridObj;
    public GameObject levelButton;
    Transform pagesTransform;

    List<MenuButton> allLevelButtons;
    
    public int maxRowsPerPage;
    int maxColumnsPerPage;
    int buttonsPerPage { get { return maxRowsPerPage * maxColumnsPerPage; }}
    int numButtons { get { return allLevelButtons.Count; }}
    int numPages { get { return (int)Mathf.Ceil((float)numButtons / (float)buttonsPerPage); }}
    
    int currentPage;
    public float pageWidth;

    public TMPro.TextMeshProUGUI levelNameText;
    public UnityEngine.UI.Button leftArrow;
    public UnityEngine.UI.Button rightArrow;
    public MenuButton backButton;

    MenuButton selectedLevel;

    bool started;
    bool mustReloadButtons;

    private void OnEnable() {
        if (mustReloadButtons) {
            LoadButtons();
            mustReloadButtons = false;
        }

        MenuButton.OnHover += OnHover;
        MenuButton.OnUnhover += OnUnhover;
    }
    private void OnDisable() {
        MenuButton.OnHover -= OnHover;
        MenuButton.OnUnhover -= OnUnhover;
    }
    public void SetReload() {
        if (started)
            mustReloadButtons = true;
    }

    protected override void Start() {
        pagesTransform = pages.GetComponent<RectTransform>();

        maxColumnsPerPage = gridObj.GetComponent<GridLayoutGroup>().constraintCount;
        currentPage = 0;

        base.Start();
        started = true;
    }

    private void Update() {
        float desiredX = currentPage * -pageWidth;
        pagesTransform.localPosition = new Vector2(Mathf.Lerp(pagesTransform.localPosition.x, desiredX, 0.2f), pagesTransform.localPosition.y);
    
        leftArrow.interactable = currentPage > 0;
        rightArrow.interactable = currentPage < numPages - 1;

        levelNameText.text = selectedLevel?.name ?? "";
    

        /* CHEAT CODE UNLOCKS ALL LEVELS: DELETE IN RELEASE
        if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.RightShift)) {
            foreach (MenuButton b in allLevelButtons)
                b.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
        */
    }

    public void LoadButtons() {
        allLevelButtons = new List<MenuButton>();

        // First, destroy the existing prototype grids
        for (int n = pagesTransform.childCount; n > 0;) {
            n--;
            Destroy(pagesTransform.GetChild(n).gameObject);
        }

        int pages = 0;
        Transform currentGrid = null;

        // Then, get the list of rooms and load buttons for each one
        List<Room> rooms = GameManager.Rooms;
        for (int r = 0; r < rooms.Count; r++) {
            // New page
            if (r >= pages * buttonsPerPage) {
                currentGrid = GameObject.Instantiate(gridObj, pagesTransform).transform;
                currentGrid.localPosition = new Vector3(pageWidth * pages, 0, 0);
                pages++;
            }

            // Place a new button at a temporary location (used for setting navigation)
            Vector3 tempButtonLocation = new Vector3((r / buttonsPerPage) * maxColumnsPerPage + (r % maxColumnsPerPage), (r % buttonsPerPage) / maxColumnsPerPage, 0);
            GameObject theButton = GameObject.Instantiate(levelButton, tempButtonLocation, Quaternion.identity, currentGrid);
            allLevelButtons.Add(theButton.GetComponent<MenuButton>());

            Room targetRoom = rooms[r];

            // Set text
            theButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (r+1) + "";
            theButton.gameObject.name = targetRoom.displayName;
            // Set color
            if (groupColors.ContainsKey(targetRoom.group))
                theButton.GetComponentInChildren<Image>().color = groupColors[targetRoom.group];
            // Set disabled
            if (r > 0 && PlayerPrefs.GetInt(targetRoom.name, 0) == 0)
                theButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            // Set click action
            theButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => LoadLevel(targetRoom.name));
        }

        firstSelection = allLevelButtons[0];
        menu?.SetPreviousSelection(firstSelection);
    }

    public override void SetNav() {
        if (hasSetNav) return;
        hasSetNav = true;

        LoadButtons();
        for (int b = 0; b < allLevelButtons.Count; b++) {
            MenuButton button = allLevelButtons[b];

            // To the right, buttons should first try to find a suitable target directly to the right, and if that doesn't work, look for one slightly displaced above
            button.Right = allLevelButtons.FirstOrDefault(o => {
                return o.GetComponent<UnityEngine.UI.Button>().interactable &&
                        button.transform.position.y == o.transform.position.y &&
                        button.transform.position.x < o.transform.position.x;
            }) ?? allLevelButtons.FirstOrDefault(o => {
                return o.GetComponent<UnityEngine.UI.Button>().interactable &&
                        button.transform.position.y < o.transform.position.y &&
                        button.transform.position.x < o.transform.position.x;
            });

            // To the left, buttons should find the suitable target directly to the left, which should always be an optimal choice
            button.Left = allLevelButtons.LastOrDefault(o => {
                return o.GetComponent<UnityEngine.UI.Button>().interactable &&
                        button.transform.position.y == o.transform.position.y &&
                        button.transform.position.x > o.transform.position.x;
            });

            // To the bottom, buttons should find the suitable target directly below, or else target the back button
            button.Down = allLevelButtons.FirstOrDefault(o => {
                return o.GetComponent<UnityEngine.UI.Button>().interactable &&
                        button.transform.position.x == o.transform.position.x &&
                        button.transform.position.y < o.transform.position.y;
            }) ?? backButton;

            // To the top, buttons should find the suitable target directly above, or else target the back button
            button.Up = allLevelButtons.LastOrDefault(o => {
                return o.GetComponent<UnityEngine.UI.Button>().interactable &&
                        button.transform.position.x == o.transform.position.x &&
                        button.transform.position.y > o.transform.position.y;
            }) ?? backButton;
        }

        backButton.Up = allLevelButtons[0];
        backButton.Down = allLevelButtons[0];
    }

    public override void DidNav(MenuButton selection) {
        int levelNumber = allLevelButtons.IndexOf(selection);
        if (levelNumber >= 0) {
            currentPage = levelNumber / buttonsPerPage;
            selectedLevel = selection;
        }
    }

    protected virtual void OnHover(PointerEventData eventData) {
        if (eventData.selectedObject != null && eventData.selectedObject.GetComponent<UnityEngine.UI.Button>().interactable) {
            DidNav(eventData.selectedObject.GetComponent<MenuButton>());
        }
    }
    protected virtual void OnUnhover(PointerEventData eventData) {
        selectedLevel = null;
    }

    // BUTTON CALLBACKS
    private void LoadLevel(string name) {
        GameManager.Instance?.LoadLevel(name);
        AudioManager.Instance?.StopAudio();
    }
    public void PageRight() {
        currentPage++;
    }
    public void PageLeft() {
        currentPage--;
    }
}
