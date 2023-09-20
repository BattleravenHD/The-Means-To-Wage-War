using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class OrderPreview : MonoBehaviour
{
    public Unit baseUnit;
    public Tile currentTile;

    public LineRenderer orderLine;

    public List<Order> orders = new List<Order>();

    private void Start()
    {
        orderLine.startColor = Color.blue;
        orderLine.endColor = Color.blue;

        if (orders.Count > 0)
        {
            orderLine.gameObject.SetActive(true);
            UpdateLines();
        }
    }

    public void UpdateLines()
    {
        orderLine.startColor = Color.blue;
        orderLine.endColor = Color.blue;

        if (orders == null || orders.Count <= 0)
        {
            orderLine.gameObject.SetActive(false);
            return;
        }else
        {
            orderLine.gameObject.SetActive(true);
        }

        List<Order> temp = new List<Order>();

        foreach (Order item in orders)
        {
            if (!item.Completed)
            {
                temp.Add(item);
                if (item is Attack)
                {
                    orderLine.startColor = Color.red;
                    orderLine.endColor = Color.red;
                }
            }
        }
        orders = temp;

        if (orders.Count == 0)
        {
            orderLine.gameObject.SetActive(false);
        }

        orderLine.positionCount = orders.Count + 1;

        orderLine.SetPosition(0, baseUnit.CurrentTile.Location - transform.position + Vector3.up);
        int index = 1;

        foreach (Order item in orders)
        {
            orderLine.SetPosition(index, item.EndTile.Location - transform.position + Vector3.up);
            index++;
        }
    }
}
