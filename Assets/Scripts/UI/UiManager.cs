using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] RectTransform MainUIArea;
    [SerializeField] RectTransform CombatUIArea;
    [SerializeField] RectTransform EndTurnUIArea;
    [SerializeField] RectTransform BuildArea;
    [SerializeField] RectTransform OrderArea;
    [SerializeField] RectTransform UnitArea;
    [SerializeField] RectTransform UnitInfoArea;
    [SerializeField] RectTransform DiploArea;
    [SerializeField] RectTransform SlideEndSpot;
    [SerializeField] RectTransform SlideStartSpot;
    [SerializeField] RectTransform UnitSlideEndSpot;
    [SerializeField] RectTransform UnitSlideStartSpot;
    [SerializeField] GameObject MainMenuArea;
    [SerializeField] TMP_Text IPCCounter;

    public bool IsPlaceing = false;
    public BuildManager buildManager;
    public CombatUIManager combatUIManager;
    public FactionData PlayerFactionData;

    bool buildMenuOpen = false;
    bool orderMenuOpen = false;
    bool diploMenuOpen = false;
    bool unitMenuOpen = false;
    bool unitInfoOpen = false;
    bool mainUIOpen = true;
    bool endTurnUiOpen = false;
    [HideInInspector] public bool combatUIOpen = false;

    private void Start()
    {
        combatUIOpen = false;
    }

    private void Update()
    {
        if (mainUIOpen)
        {
            if (Vector2.Distance(MainUIArea.localScale, new Vector3(1,1,1)) > 0.1f)
            {
                MainUIArea.localScale = Vector3.Lerp(MainUIArea.localScale, new Vector3(1, 1, 1), 0.1f);
            }
            else
            {
                MainUIArea.localScale = new Vector3(1, 1, 1);
            }

            if (buildMenuOpen)
            {
                if (Vector2.Distance(BuildArea.localPosition, SlideStartSpot.localPosition) > 10)
                {
                    BuildArea.localPosition = Vector2.Lerp(BuildArea.localPosition, SlideStartSpot.localPosition, 0.3f);

                }
                else
                {
                    BuildArea.localPosition = SlideStartSpot.localPosition;
                }
            }
            else
            {
                if (Vector2.Distance(BuildArea.localPosition, SlideEndSpot.localPosition) > -490)
                {
                    BuildArea.localPosition = Vector2.Lerp(BuildArea.localPosition, SlideEndSpot.localPosition, 0.3f);
                }
                else
                {
                    BuildArea.localPosition = SlideEndSpot.localPosition;
                }
            }

            if (unitInfoOpen)
            {
                if (Vector2.Distance(UnitInfoArea.localPosition, UnitSlideStartSpot.localPosition) > 10)
                {
                    UnitInfoArea.localPosition = Vector2.Lerp(UnitInfoArea.localPosition, UnitSlideStartSpot.localPosition, 0.3f);

                }
                else
                {
                    UnitInfoArea.localPosition = UnitSlideStartSpot.localPosition;
                }
            }
            else
            {
                if (Vector2.Distance(UnitInfoArea.localPosition, UnitSlideEndSpot.localPosition) > -490)
                {
                    UnitInfoArea.localPosition = Vector2.Lerp(UnitInfoArea.localPosition, UnitSlideEndSpot.localPosition, 0.3f);
                }
                else
                {
                    UnitInfoArea.localPosition = UnitSlideEndSpot.localPosition;
                }
            }
        }else
        {
            if (Vector2.Distance(MainUIArea.localScale, new Vector3(3, 3, 3)) > 0.1f)
            {
                MainUIArea.localScale = Vector3.Lerp(MainUIArea.localScale, new Vector3(3, 3, 3), 0.1f);
            }
            else
            {
                MainUIArea.localScale = new Vector3(3, 3, 3);
            }
        }

        if (combatUIOpen)
        {
            if (Vector2.Distance(CombatUIArea.localScale, new Vector3(1, 1, 1)) > 0.1f)
            {
                CombatUIArea.localScale = Vector3.Lerp(CombatUIArea.localScale, new Vector3(1, 1, 1), 0.1f);
            }
            else
            {
                CombatUIArea.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            if (Vector2.Distance(CombatUIArea.localScale, new Vector3(10, 10, 10)) > 0.1f)
            {
                CombatUIArea.localScale = Vector3.Lerp(CombatUIArea.localScale, new Vector3(10, 10, 10), 0.1f);
            }
            else
            {
                CombatUIArea.localScale = new Vector3(10, 10, 10);
            }
        }
        IPCCounter.text = PlayerFactionData.FactionIPC.ToString();
        if (endTurnUiOpen)
        {
            if (Vector2.Distance(EndTurnUIArea.localScale, new Vector3(1, 1, 1)) > 0.1f)
            {
                EndTurnUIArea.localScale = Vector3.Lerp(EndTurnUIArea.localScale, new Vector3(1, 1, 1), 0.1f);
            }
            else
            {
                EndTurnUIArea.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            if (Vector2.Distance(EndTurnUIArea.localScale, new Vector3(3, 3, 3)) > 0.1f)
            {
                EndTurnUIArea.localScale = Vector3.Lerp(EndTurnUIArea.localScale, new Vector3(3, 3, 3), 0.1f);
            }
            else
            {
                EndTurnUIArea.localScale = new Vector3(3, 3, 3);
            }
        }
    }

    public void OpenBuildMenu()
    {
        bool old = buildMenuOpen;
        CloseMenu();
        buildMenuOpen = !old;
    }

    public void OpenOrderMenu()
    {
        bool old = orderMenuOpen;
        CloseMenu();
        orderMenuOpen = !old;
    }

    public void OpenUnitMenu()
    {
        bool old = unitMenuOpen;
        CloseMenu();
        unitMenuOpen = !old;
    }

    public void OpenDiplomacyMenu()
    {
        bool old = diploMenuOpen;
        CloseMenu();
        diploMenuOpen = !old;
    }

    public void OpenUnitInfo()
    {
        unitInfoOpen = true;
    }

    public void CloseMenu()
    {
        buildMenuOpen = false;
        orderMenuOpen = false;
        diploMenuOpen = false;
        unitMenuOpen = false;
    }

    public void CloseUnitInfo()
    {
        unitInfoOpen = false;
    }

    public void OpenCombat()
    {
        combatUIOpen = true;
        mainUIOpen = false;
        endTurnUiOpen = false;
    }

    public void CloseCombat()
    {
        combatUIOpen = false;
        mainUIOpen = false;
        endTurnUiOpen = true;
    }

    public void EndTurn()
    {
        CloseMenu();
        combatUIOpen = false;
        mainUIOpen = false;
        endTurnUiOpen = true;
    }

    public void NewTurn()
    {
        CloseMenu();
        combatUIOpen = false;
        mainUIOpen = true;
        endTurnUiOpen = false;
    }

    public static void SetLeft(RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
}
