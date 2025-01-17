using UnityEngine;
using Mono.Data.Sqlite;
using System.IO;
using System;
using System.Collections.Generic;

public class MyDBManager
{

    private string dbFilePath = Application.dataPath + "/../FanRenData/originData.db";
    private SqliteConnection mSqliteConnection;
    private static MyDBManager mMyDBManager = new MyDBManager();

    private bool mIsConnected = false;

    private MyDBManager()
    {
    }

    
    public static MyDBManager GetInstance()
    {
        return mMyDBManager;
    }

    /// <summary>
    /// todo 有新增RW表需要在这里添加
    /// 人物信息表，虽然是只读，但是主角和他的帮手（傀儡之类）也都暂时存在这个表 
    /// </summary>
    public void DeleteAllRWGameData()
    {
        string[] rwTalbeName = { "role_active_gongfa_rw", 
            "role_active_shentong_rw", 
            "role_bag_rw", 
            "role_tasks_rw", 
            "role_info_rw",
            "place_info_rw",
            "npc_collection_rw" 
        };

        //sqlite不支持truncate
        //sqliteCommand.CommandText = $"truncate table role_active_gongfa_rw,role_active_shentong_rw,role_bag_rw,role_tasks_rw";
        foreach(string tableName in rwTalbeName)
        {
            SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"delete from {tableName}";
            sqliteCommand.ExecuteNonQuery();
            sqliteCommand.Dispose();
        }
    }

    public bool ConnDB()
    {
        if (this.mIsConnected) return true;
        try
        {
            if (!Directory.Exists(new FileInfo(dbFilePath).Directory.FullName))
            {
                Directory.CreateDirectory(new FileInfo(dbFilePath).Directory.FullName);
            }
            if (!File.Exists(dbFilePath))
            {
                SqliteConnection.CreateFile(dbFilePath);
            }
            if (mSqliteConnection == null)
            {
                mSqliteConnection = new SqliteConnection(new SqliteConnectionStringBuilder() { DataSource = dbFilePath }.ToString());
            }
            mSqliteConnection.Open();
            this.mIsConnected = true;
            return this.mIsConnected;
        }
        catch (Exception e)
        {
            Debug.LogError("ConnDB error : " + e.ToString());
            this.mIsConnected = false;
            return this.mIsConnected;
        }
    }

    public bool IsConnected()
    {
        return this.mIsConnected;
    }

    private string TryGetStringValue(SqliteDataReader sdr, string key)
    {
        return sdr[key].Equals(DBNull.Value) ? "" : (string)sdr[key];
    }

    public RoleInfo GetRoleInfo(int roleId)
    {
        RoleInfo roleInfo = null;
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        if(roleId == 1)
        {
            sqliteCommand.CommandText = $"select * from role_info_rw where roleId={roleId}";
        }
        else
        {
            sqliteCommand.CommandText = $"select * from role_info_r where roleId={roleId}";
        }
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            roleInfo = new RoleInfo();
            //public int roleId;
            string roleName = (string)sdr["roleName"];
            string roleAvatar = (string)sdr["roleAvatar"];
            string battleModelPath = (string)sdr["battleModelPath"];
            int hp = (int)((Int64)sdr["hp"]);
            int maxHp = (int)((Int64)sdr["maxHp"]);
            int mp = (int)((Int64)sdr["mp"]);
            int maxMp = (int)((Int64)sdr["maxMp"]);
            int speed = (int)((Int64)sdr["speed"]);
            int gongJiLi = (int)((Int64)sdr["attack"]);
            int fangYuLi = (int)((Int64)sdr["defense"]);

            string canGetItemId = TryGetStringValue(sdr, "canGetItemId");
            string canGetItemPercent = TryGetStringValue(sdr, "canGetItemPercent");

            roleInfo.roleId = roleId;
            roleInfo.roleName = roleName;
            roleInfo.roleAvatar = roleAvatar;
            roleInfo.currentHp = hp;
            roleInfo.maxHp = maxHp;
            roleInfo.currentMp = mp;
            roleInfo.maxMp = maxMp;
            roleInfo.speed = speed;
            roleInfo.gongJiLi = gongJiLi;
            roleInfo.fangYuLi = fangYuLi;
            roleInfo.battleModelPath = battleModelPath;

            roleInfo.canGetItemId = canGetItemId;
            roleInfo.canGetItemPercent = canGetItemPercent;

        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleInfo;
    }

    public bool UpdateRoleInfo(int roleId, int nowHp, int nowMp)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"update role_info_rw set hp={nowHp}, mp={nowMp} where roleId={roleId}";
        bool result = sqliteCommand.ExecuteNonQuery() == 1;
        sqliteCommand.Dispose();
        return result;
    }

    public bool InsertRoleInfo(int roleId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"insert into role_info_rw select * from role_info_r where roleId={roleId}";
        bool result = sqliteCommand.ExecuteNonQuery() == 1;
        sqliteCommand.Dispose();
        return result;
    }

    //isActive 学完后，是否有装备上
    public void ZhujueLearnShentong(int shenTongId, int isActive)
    {
        //SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        //sqliteCommand.CommandText = $"insert into role_active_shentong_rw (shenTongId, isActive, roleId) values ({shenTongId}, {isActive}, 1)";
        //sqliteCommand.ExecuteNonQuery();
        LearnShentong(1, shenTongId, isActive);
    }

    public void LearnShentong(int roleId, int shenTongId, int isActive)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"insert into role_active_shentong_rw (shenTongId, isActive, roleId) values ({shenTongId}, {isActive}, {roleId})";
        sqliteCommand.ExecuteNonQuery();
    }

    //activeState 0查询全部 1只查询激活的
    //isZhuJue 是否是查询主角的数据， 否则查询NPC的数据，两者在不同的表
    public List<Shentong> GetRoleShentong(int roleId, int activeState, bool isZhuJue)
    {
        List<Shentong> roleShentong = new List<Shentong>();
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        string tableName = isZhuJue ? "role_active_shentong_rw" : "role_active_shentong_r";
        sqliteCommand.CommandText = $"select * from {tableName} a left join shen_tong_r b on a.shenTongId=b.id where a.roleId={roleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        while (sdr.Read())
        {
            int isActive = (int)((Int64)sdr["isActive"]);
            if(activeState == 1) //只查询激活
            {
                if (isActive != 1) continue;
            }

            int shenTongId = (int)((Int64)sdr["shenTongId"]);
            
            //int roleId = (int)((Int64)sdr["roleId"]);

            string shenTongName = (string)sdr["name"];
            int damage = (int)((Int64)sdr["damage"]);
            int defence = (int)((Int64)sdr["defense"]);
            string desc = (string)sdr["desc"];
            int studyRequireLevel = (int)((Int64)sdr["studyRequireLevel"]);
            int effType = (int)((Int64)sdr["effType"]); //神通类型，攻击、防御、变身 等等
            int rangeType = (int)((Int64)sdr["rangeType"]); //攻击范围类型，一条、一个面、一个点 等等
            int planeRadius = (int)((Int64)sdr["planeRadius"]); //面类型的攻击范围“半径”
            string effPath = (string)sdr["effPath"];
            string soundEffPath = (string)sdr["soundEffPath"];
            int unitDistance = (int)((Int64)sdr["unitDistance"]); //神通攻击距离
            int needMp = (int)((Int64)sdr["needMp"]);

            Shentong shenTong = new Shentong();
            shenTong.shenTongId = shenTongId;
            shenTong.isActive = isActive;
            shenTong.roleId = roleId;

            shenTong.shenTongName = shenTongName;
            shenTong.damage = damage;
            shenTong.defence = defence;
            shenTong.desc = desc;
            shenTong.studyRequireLevel = studyRequireLevel;
            shenTong.effType = (ShentongEffType)effType;
            shenTong.rangeType = (ShentongRangeType)rangeType;
            shenTong.planeRadius = planeRadius;
            shenTong.effPath = effPath;
            shenTong.soundEffPath = soundEffPath;
            shenTong.unitDistance = unitDistance;
            shenTong.needMp = needMp;

            roleShentong.Add(shenTong);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleShentong;
    }

    public List<RoleItem> GetRoleItemInBag(int roleId, bool isForBattle)
    {
        List<RoleItem> allRoleItem = new List<RoleItem>();
        RoleItem roleItem;
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_bag_rw a left join items_r b on a.itemId=b.itemId where a.roleId={roleId}";
        if (isForBattle) sqliteCommand.CommandText += " and (b.recoverHp>0 or b.recoverMp>0)";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        while (sdr.Read())
        {
            roleItem = new RoleItem();

            roleItem.roleId = roleId;
            roleItem.itemId = (int)((Int64)sdr["itemId"]);
            roleItem.itemCount = (int)((Int64)sdr["itemCount"]); //比GetItemDetailInfo仅多出这一项

            roleItem.itemType = (int)((Int64)sdr["itemType"]);
            roleItem.itemName = (string)(sdr["itemName"]);

            roleItem.addPhyAck = (int)((Int64)sdr["addPhyAck"]);
            roleItem.addPhyDef = (int)((Int64)sdr["addPhyDef"]);
            roleItem.price = (int)((Int64)sdr["price"]);

            roleItem.imageName = (string)(sdr["imageName"]);
            roleItem.itemDesc = (string)(sdr["itemDesc"]);

            roleItem.scarceLevel = (int)((Int64)sdr["scarceLevel"]);

            roleItem.recoverHp = (int)((Int64)sdr["recoverHp"]);
            roleItem.recoverMp = (int)((Int64)sdr["recoverMp"]);

            allRoleItem.Add(roleItem);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return allRoleItem;
    }

    public RoleItem GetRoleItemInBag(int itemId)
    {
        RoleItem roleItem = new RoleItem();
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_bag_rw a left join items_r b on a.itemId=b.itemId where a.itemId={itemId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            roleItem.itemId = itemId;
            roleItem.itemCount = (int)((Int64)sdr["itemCount"]); //比GetItemDetailInfo仅多出这一项

            roleItem.itemType = (int)((Int64)sdr["itemType"]);
            roleItem.itemName = (string)(sdr["itemName"]);

            roleItem.addPhyAck = (int)((Int64)sdr["addPhyAck"]);
            roleItem.addPhyDef = (int)((Int64)sdr["addPhyDef"]);
            roleItem.price = (int)((Int64)sdr["price"]);

            roleItem.imageName = (string)(sdr["imageName"]);
            roleItem.itemDesc = (string)(sdr["itemDesc"]);

            roleItem.scarceLevel = (int)((Int64)sdr["scarceLevel"]);

            roleItem.recoverHp = (int)((Int64)sdr["recoverHp"]);
            roleItem.recoverMp = (int)((Int64)sdr["recoverMp"]);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleItem;
    }

    public RoleItem GetItemDetailInfo(int itemId)
    {
        RoleItem roleItem = new RoleItem();
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from items_r where itemId={itemId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            roleItem.itemId = itemId;
            //roleItem.itemCount = (int)((Int64)sdr["itemCount"]);
            roleItem.itemType = (int)((Int64)sdr["itemType"]);
            roleItem.itemName = (string)(sdr["itemName"]);

            roleItem.addPhyAck = (int)((Int64)sdr["addPhyAck"]);
            roleItem.addPhyDef = (int)((Int64)sdr["addPhyDef"]);
            roleItem.price = (int)((Int64)sdr["price"]);

            roleItem.imageName = (string)(sdr["imageName"]);
            roleItem.itemDesc = (string)(sdr["itemDesc"]);

            roleItem.scarceLevel = (int)((Int64)sdr["scarceLevel"]);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleItem;
    }


    public bool AddRoleTask(int taskId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_tasks_rw where taskId={taskId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            //任务已经存在
            Debug.Log("任务已经存在 taskId " + taskId);
            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            return true;
        }
        else
        {
            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            SqliteCommand sqliteCommand2 = this.mSqliteConnection.CreateCommand();
            sqliteCommand2.CommandText = $"insert into role_tasks_rw (taskId, taskState) values ({taskId}, {((int)FRTaskState.InProgress)})";
            bool result = sqliteCommand2.ExecuteNonQuery() == 1;
            sqliteCommand2.Dispose();
            return result;
        }
    }

    //查询某个任务
    public RoleTask GetRoleTask(int taskId)
    {
        Debug.Log("GetRoleTask taskId : " + taskId);
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand(); 
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId=b.taskId where a.taskId={taskId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = taskId;
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : (int)((Int64)sdr["taskState"]);
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            return roleTask;
        }
        else
        {
            Debug.LogError("逻辑错误，查无数据 GetRoleTask taskId : " + taskId);
            return null;
        }
    }

    /**
     * 查询某个已经触发的任务
      **/
    public RoleTask GetTriggedRoleTask(int taskId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId=b.taskId where a.taskId={taskId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        if (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = taskId;
            roleTask.taskState = (int)((Int64)sdr["taskState"]);
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            sdr.Close();
            sdr.Dispose();
            sqliteCommand.Dispose();

            return roleTask;
        }
        else
        {
            return null;
        }
    }

    //获取某个角色能触发的所有任务
    public List<RoleTask> GetRoleTasks(int roleId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId=b.taskId where a.triggerRoleId={roleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        List<RoleTask> results = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]); 
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : ((int)((Int64)sdr["taskState"]));
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = roleId;
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            results.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return results;
    }

    public bool UpdateRoleTaskState(int taskId, FRTaskState taskState)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"update role_tasks_rw set taskState={((int)taskState)} where taskId={taskId}";
        bool result = sqliteCommand.ExecuteNonQuery() == 1;
        sqliteCommand.Dispose();
        return result;
    }

    //查询所有进行中的任务
    public List<RoleTask> GetAllLeaderActorInProgressTasks()
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId = b.taskId where a.taskState={((int)FRTaskState.InProgress)}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();

        List<RoleTask> roleTasks = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]);
            roleTask.taskState = (int)((Int64)sdr["taskState"]);
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            roleTasks.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleTasks;
    }

    //查询某NPC能触发的所有任务
    public List<RoleTask> GetAllLeaderActorWithNPCTriggerTasks(int triggerNPCRoleId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        //sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId = b.taskId where a.taskState={((int)state)} and b.triggerRoleId={triggerNPCRoleId}";
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId = b.taskId where a.triggerRoleId={triggerNPCRoleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        List<RoleTask> roleTasks = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]);
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : ((int)((Int64)sdr["taskState"]));
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = triggerNPCRoleId;
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            roleTasks.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleTasks;
    }

    //查询某NPC能提交的所有任务
    public List<RoleTask> GetAllLeaderActorWithNPCSubmitTasks(int submitNPCRoleId)
    {
        SqliteCommand sqliteCommand = this.mSqliteConnection.CreateCommand();
        //sqliteCommand.CommandText = $"select * from role_tasks_rw a left join tasks_r b on a.taskId = b.taskId where a.taskState={((int)state)} and b.triggerRoleId={triggerNPCRoleId}";
        sqliteCommand.CommandText = $"select * from tasks_r a left join role_tasks_rw b on a.taskId = b.taskId where a.submitRoleId={submitNPCRoleId}";
        SqliteDataReader sdr = sqliteCommand.ExecuteReader();
        List<RoleTask> roleTasks = new List<RoleTask>();
        while (sdr.Read())
        {
            RoleTask roleTask = new RoleTask();
            roleTask.taskId = (int)((Int64)sdr["taskId"]);
            roleTask.taskState = sdr["taskState"].Equals(DBNull.Value) ? (int)FRTaskState.Untrigger : ((int)((Int64)sdr["taskState"]));
            roleTask.remark = (string)sdr["remark"];
            roleTask.isMainTask = (int)((Int64)sdr["isMainTask"]);
            roleTask.storyLineIndex = (int)((Int64)sdr["storyLineIndex"]);
            roleTask.triggerRoleId = (int)((Int64)sdr["triggerRoleId"]);
            roleTask.submitRoleId = (int)((Int64)sdr["submitRoleId"]);

            roleTasks.Add(roleTask);
        }
        sdr.Close();
        sdr.Dispose();
        sqliteCommand.Dispose();
        return roleTasks;
    }

    public bool AddItemToBag(int itemId, int addCount)
    {
        SqliteCommand sqliteCommand = null;
        SqliteCommand sqliteCommand2 = null;
        SqliteDataReader sdr = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"select * from role_bag_rw where itemId={itemId}";
            sdr = sqliteCommand.ExecuteReader();

            sqliteCommand2 = this.mSqliteConnection.CreateCommand();
            if (sdr.Read()) //包里已经有该道具，只需要增加数量即可
            {
                Int64 originCount = (Int64)sdr["itemCount"];
                Int64 resultCount = originCount + addCount;
                sqliteCommand2.CommandText = $"update role_bag_rw set itemCount={resultCount} where itemId={itemId}";
            }
            else
            {
                //insert
                sqliteCommand2.CommandText = $"insert into role_bag_rw (itemId, itemCount) values ({itemId}, {addCount})";
            }
            bool result = sqliteCommand2.ExecuteNonQuery() == 1;
            return result;
        }
        catch(Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return false;
        }
        finally
        {
            if(sdr != null)
            {
                sdr.Close();
                sdr.Dispose();
            }
            if(sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
            if (sqliteCommand2 != null)
            {
                sqliteCommand2.Dispose();
            }
        }
    }

    public bool DeleteItemInBag(int itemId, int deleteCount, int ownCount)
    {
        SqliteCommand sqliteCommand = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            if(deleteCount == ownCount)
            {
                sqliteCommand.CommandText = $"delete from role_bag_rw where itemId={itemId}";
            }
            else if(deleteCount < ownCount)
            {
                sqliteCommand.CommandText = $"update role_bag_rw set itemCount={ownCount-deleteCount} where itemId={itemId}";
            }
            else
            {
                Debug.LogError("DeleteItemInBag 逻辑错误");
                return false;
            }
            return sqliteCommand.ExecuteNonQuery() == 1;
        }
        catch (Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return false;
        }
        finally
        {
            if (sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
        }
    }

    public bool InsertCollectionPlace(int placeId)
    {
        SqliteCommand sqliteCommand = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"insert into place_info_rw (placeId) values({placeId})";
            return sqliteCommand.ExecuteNonQuery() == 1;
        }
        catch (Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return false;
        }
        finally
        {
            if (sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
        }
    }

    public List<PlaceCollectionEntity> GetAllCollectionPlace()
    {
        List<PlaceCollectionEntity> allPlace = new List<PlaceCollectionEntity>();
        SqliteCommand sqliteCommand = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"select * from place_info_r a left join place_info_rw b on a.id=b.placeId";
            SqliteDataReader sdr = sqliteCommand.ExecuteReader();
            PlaceCollectionEntity placeCollectionEntity = null;
            while (sdr.Read())
            {
                placeCollectionEntity = new PlaceCollectionEntity();
                placeCollectionEntity.id = (int)((Int64)sdr["placeId"]);
                placeCollectionEntity.placeName = (string)sdr["placeName"];
                placeCollectionEntity.desc = (string)sdr["desc"];
                placeCollectionEntity.imagePath = (string)sdr["imagePath"];
                placeCollectionEntity.isOpened = sdr["placeId"].Equals(DBNull.Value) ? false : true;
                allPlace.Add(placeCollectionEntity);
            }
            return allPlace;
        }
        catch (Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return allPlace;
        }
        finally
        {
            if (sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
        }
    }

    public bool InsertCollectionNPC(int npcId)
    {
        SqliteCommand sqliteCommand = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"insert into npc_collection_rw (npcCollectionId) values({npcId})";
            return sqliteCommand.ExecuteNonQuery() == 1;
        }
        catch (Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return false;
        }
        finally
        {
            if (sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
        }
    }

    public List<NPCCollectionEntity> GetAllCollectionNPC()
    {
        List<NPCCollectionEntity> allPlace = new List<NPCCollectionEntity>();
        SqliteCommand sqliteCommand = null;
        try
        {
            sqliteCommand = this.mSqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"select * from npc_collection_r a left join npc_collection_rw b on a.id=b.npcCollectionId";
            SqliteDataReader sdr = sqliteCommand.ExecuteReader();
            NPCCollectionEntity npcCollectionEntity = null;
            while (sdr.Read())
            {
                npcCollectionEntity = new NPCCollectionEntity();
                npcCollectionEntity.id = sdr["npcCollectionId"].Equals(DBNull.Value) ? 0 : (int)((Int64)sdr["npcCollectionId"]);
                npcCollectionEntity.npcName = (string)sdr["npcName"];
                npcCollectionEntity.desc = (string)sdr["desc"];
                npcCollectionEntity.imagePath = (string)sdr["imagePath"];
                npcCollectionEntity.isOpened = sdr["npcCollectionId"].Equals(DBNull.Value) ? false : true;
                allPlace.Add(npcCollectionEntity);
            }
            return allPlace;
        }
        catch (Exception e)
        {
            Debug.LogError("AddItemToBag, " + e.ToString());
            return allPlace;
        }
        finally
        {
            if (sqliteCommand != null)
            {
                sqliteCommand.Dispose();
            }
        }
    }

}

public interface BaseEntity
{
}

public class PlaceCollectionEntity
{
    public int id;
    public string placeName;
    public string desc;
    public string imagePath;
    public bool isOpened;
}

public class NPCCollectionEntity
{
    public int id;
    public string npcName;
    public string desc;
    public string imagePath;
    public bool isOpened;
}

//角色任务
public class RoleTask
{
    public int taskId;
    public int taskState;
    public string remark;
    public int isMainTask;
    public int storyLineIndex;
    public int triggerRoleId;
    public int submitRoleId;
}

//角色信息
public class RoleInfo
{
    public int roleId;
    public string roleName;
    public int currentHp;
    public int maxHp;
    public int currentMp;
    public int maxMp;
    public int speed;
    public int gongJiLi;
    public int fangYuLi;
    public string roleAvatar;
    public string battleModelPath;

    /// <summary>
    /// 击败可以获取的道具id
    /// </summary>
    public string canGetItemId; 
    /// <summary>
    /// 击败获取道具的概率
    /// </summary>
    public string canGetItemPercent;

    public List<int> CanGetItemIdList()
    {
        if (canGetItemId == null || canGetItemId.Trim().Length == 0) return null;
        List<int> result = new List<int>();
        string[] idStringArray = canGetItemId.Split(",");
        foreach(string itemId in idStringArray)
        {
            result.Add(int.Parse(itemId));
        }
        return result;
    }

    public List<float> CanGetItemIdPercentList()
    {
        if (canGetItemPercent == null || canGetItemPercent.Trim().Length == 0) return null;
        List<float> result = new List<float>();
        string[] percentStringArray = canGetItemPercent.Split(",");
        foreach (string gainPercent in percentStringArray)
        {
            result.Add(float.Parse(gainPercent));
        }
        return result;
    }
}

//角色拥有的物品
public class RoleItem
{
    public int roleId;
    public int itemId;
    public int itemType;
    public int itemCount;
    public string itemName;
    public int addPhyAck;
    public int addPhyDef;
    public int price;
    public string imageName;
    public string itemDesc;
    public int scarceLevel;

    public int recoverHp;
    public int recoverMp;
}

//任务状态
public enum FRTaskState
{
    InProgress = 1, //进行种
    Finished = 2, //完成
    Fail = 3, //失败
    Untrigger = 0 //还没有触发
}

//物品类型
public enum FRItemType
{
    Fabao = 1,//法宝
    CaiLiao = 2,//材料
    LingCao = 3,//灵草
    DanYao = 4,//丹药
    LingShou = 5,//灵兽
    LingChong = 6,//灵虫
    GongFa = 7,//功法
    DanFang = 8,//丹方
    Other = 9,//其他
    KuiLei = 10,//傀儡
    TianDiLingWu = 11,//天地灵物
    ShenTong = 12,//神通
    Story = 13//剧情
}