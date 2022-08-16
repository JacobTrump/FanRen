using System.Collections.Generic;
using UnityEngine;

public class AStarPathUtil
{

    public class Node
    {
        public int x;
        public int y;
        public int g;
        public int h;
        public int f;
        public Node preNode;
        public Node(int x, int y, Node preNode, (int, int) target)
        {
            this.x = x;
            this.y = y;
            this.preNode = preNode;
            this.g = preNode != null ? (preNode.g + 1) : 0; //不允许斜着行走，所以全部是固定1
            this.h = Mathf.Abs(this.x - target.Item1) + Mathf.Abs(this.y - target.Item2);
            this.f = this.g + this.h;
        }

    }

    public int mapWidth;
    public int mapHeight;
    public (int, int) start;
    public (int, int) target;
    public List<(int, int)> obstacles;

    private List<Node> openList = new List<Node>();
    private List<Node> closeList = new List<Node>();

    public AStarPathUtil()
    {
    }

    public AStarPathUtil(int mapWidth, int mapHeight, (int, int) start, (int, int) target, List<(int, int)> obstacles)
    {
        Reset(mapWidth, mapHeight, start, target, obstacles);
    }

    public void Reset(int mapWidth, int mapHeight, (int, int) start, (int, int) target, List<(int, int)> obstacles)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.start = start;
        this.target = target;
        this.obstacles = obstacles;
    }

    public List<Node> GetShortestPath()
    {
        Node startNode = new Node(start.Item1, start.Item2, null, target);
        closeList.Add(startNode);

        List<Node> neighbors = GetNewValidityNeighbors(startNode);
        //检查openlist中有没终点，有则结束
        foreach (Node item in neighbors)
        {
            if (target.Item1 == item.x && target.Item2 == item.y)
            {
                List<Node> path = new List<Node>();
                path.Add(startNode);
                return path;
            }
        }
        openList.AddRange(neighbors);
        //================初始化完成



        Node smallestF;
        while ((smallestF = GetSmallestFValueFromOpenList()) != null)
        {
            openList.Remove(smallestF);
            closeList.Add(smallestF);

            neighbors = GetNewValidityNeighbors(smallestF);
            if (neighbors.Count == 0) continue;

            //检查新邻居中有没终点，有则结束
            foreach (Node item in neighbors)
            {
                if (target.Item1 == item.x && target.Item2 == item.y)
                {
                    Stack<Node> path = new Stack<Node>();
                    List<Node> pathOrderByStartToEnd = new List<Node>();
                    Node node = item;
                    while (node.preNode != null)
                    {
                        node = node.preNode;
                        path.Push(node);
                    }
                    while (path.Count > 0)
                    {
                        pathOrderByStartToEnd.Add(path.Pop());
                    }
                    return pathOrderByStartToEnd;
                }
            }

            openList.AddRange(neighbors);
        }

        //不存在路径
        return new List<Node>();

    }

    private Node GetSmallestFValueFromOpenList()
    {
        if (openList.Count == 0) return null;
        int f = openList[0].f;
        int index = 0;
        for(int i=1; i< openList.Count; i++)
        {
            if (openList[i].f < f)
            {
                f = openList[i].f;
                index = i;
            }
        }
        return openList[index];
    }

    private List<Node> GetNewValidityNeighbors(Node node)
    {
        List<Node> nodes = new List<Node>();
        if (IsValidityPosition(node.x, node.y + 1))
        {
            nodes.Add(new Node(node.x, node.y + 1, node, target));
        }
        if (IsValidityPosition(node.x, node.y - 1))
        {
            nodes.Add(new Node(node.x, node.y - 1, node, target));
        }
        if (IsValidityPosition(node.x+1, node.y))
        {
            nodes.Add(new Node(node.x+1, node.y, node, target));
        }
        if (IsValidityPosition(node.x-1, node.y))
        {
            nodes.Add(new Node(node.x-1, node.y, node, target));
        }
        return nodes;
    }

    private bool IsValidityPosition(int x, int y)
    {
        //超出界限的位置
        if (x > mapWidth - 1) return false;
        if (x < 0) return false;
        if (y > mapHeight - 1) return false;
        if (y < 0) return false;

        //障碍物的位置
        if(obstacles != null && obstacles.Count > 0)
        {
            foreach ((int, int) wrongItem in obstacles)
            {
                if (wrongItem.Item1 == x && wrongItem.Item2 == y)
                {
                    return false;
                }
            }
        }

        foreach (Node item in openList)
        {
            if (x == item.x && y == item.y)
            {
                return false;
            }
        }

        foreach (Node item in closeList)
        {
            if (x == item.x && y == item.y)
            {
                return false;
            }
        }

        return true;
    }

}
