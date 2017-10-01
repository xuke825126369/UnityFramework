using System.Collections;
using System.Collections.Generic;
namespace xk_System.Db
{
	public class HeroAttDB:DbBase
	{
		/// <summary>
		/// 英雄ID
		/// </summary>
		public readonly int heroId;
		/// <summary>
		/// 英雄名
		/// </summary>
		public readonly string heroName;
		/// <summary>
		/// 英雄Atlas
		/// </summary>
		public readonly string heroAtlas;
		/// <summary>
		/// 英雄Icon数组
		/// </summary>
		public readonly string heroIcon;
		/// <summary>
		/// 英雄模型Bundle
		/// </summary>
		public readonly string heroModelBundle;
		/// <summary>
		/// 英雄模型名
		/// </summary>
		public readonly string HeroModelName;
		/// <summary>
		/// 基础生命值
		/// </summary>
		public readonly int Hp;
		/// <summary>
		/// 等级加成生命值千分比
		/// </summary>
		public readonly int Level;
		/// <summary>
		/// 法力资源类型（0：不消耗1：法力值2：能量值3：怒气）
		/// </summary>
		public readonly int MPType;
		/// <summary>
		/// 基础法力值
		/// </summary>
		public readonly int MPValue;
		/// <summary>
		/// 等级加成法力值百分比
		/// </summary>
		public readonly int MP;
		/// <summary>
		/// 基础攻击力
		/// </summary>
		public readonly int bbb3;
		/// <summary>
		/// 等级加成攻击力百分比
		/// </summary>
		public readonly int LevelAddAttackPercent;
		/// <summary>
		/// 基础法强
		/// </summary>
		public readonly int bbb4;
		/// <summary>
		/// 等级加成法强百分比
		/// </summary>
		public readonly int LevelAddMAttackPercent;
		/// <summary>
		/// 基础护甲
		/// </summary>
		public readonly int bbb5;
		/// <summary>
		/// 等级加成护甲百分比
		/// </summary>
		public readonly int LevelAddDefPercent;
		/// <summary>
		/// 等级加成魔抗百分比
		/// </summary>
		public readonly int LevelAddMDefPercent;
		/// <summary>
		/// 基础移动速度
		/// </summary>
		public readonly int MoveSpeed;
		/// <summary>
		/// 等级加成移动速度
		/// </summary>
		public readonly int LevelAddMoveSpeed;
		/// <summary>
		/// 基础回复生命值速度
		/// </summary>
		public readonly int HPSpeed;
		/// <summary>
		/// 等级加成基础生命值回复速度
		/// </summary>
		public readonly int LevelAddHpSpeedPercent;
		/// <summary>
		/// 基础法力资源回复速度
		/// </summary>
		public readonly int qweqe;
		/// <summary>
		/// 等级加成法力资源回复速度
		/// </summary>
		public readonly int t;
	}

	public class ItemDB:DbBase
	{
		/// <summary>
		/// 物品类型
		/// </summary>
		public readonly List<int> TypeArray=new List<int>();
		/// <summary>
		/// 物品类型名
		/// </summary>
		public readonly string ItemTypeName;
		/// <summary>
		/// 物品名称
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// 子物品
		/// </summary>
		public readonly List<int> SubItemArray=new List<int>();
		/// <summary>
		/// 合成价格
		/// </summary>
		public readonly int CompoundPrice;
		/// <summary>
		/// 物品出售价格百分比
		/// </summary>
		public readonly int SellPricePercent;
		/// <summary>
		/// 物品叠加数量最大值
		/// </summary>
		public readonly int pileCout;
		/// <summary>
		/// 物品限制组ID
		/// </summary>
		public readonly int ItemLimitId;
		/// <summary>
		/// 属性
		/// </summary>
		public readonly List<int> ItemAttGroup=new List<int>();
		/// <summary>
		/// 技能
		/// </summary>
		public readonly List<int> skillGroup=new List<int>();
		/// <summary>
		/// 物品简单描述
		/// </summary>
		public readonly string simpleDes;
	}

	public class ServerListDB:DbBase
	{
		/// <summary>
		/// 区ID
		/// </summary>
		public readonly int serverId;
		/// <summary>
		/// 区名
		/// </summary>
		public readonly string serverName;
	}

	public class SkillDB:DbBase
	{
		/// <summary>
		/// 技能所属对象类型
		/// </summary>
		public readonly int ownerType;
		/// <summary>
		/// 技能所属对象ID
		/// </summary>
		public readonly int ownerID;
		/// <summary>
		/// 技能所属对象名
		/// </summary>
		public readonly string ownerName;
		/// <summary>
		/// 技能名字
		/// </summary>
		public readonly string skillName;
		/// <summary>
		/// 被动技能互斥组ID
		/// </summary>
		public readonly int exclusionSkillGroupId;
		/// <summary>
		/// 技能类型（1被动，2主动）
		/// </summary>
		public readonly int skilltype;
		/// <summary>
		/// 技能CD
		/// </summary>
		public readonly int skillCD;
		/// <summary>
		/// 技能描述
		/// </summary>
		public readonly string skillDes;
		/// <summary>
		/// 参数1
		/// </summary>
		public readonly int arg1;
		/// <summary>
		/// 参数2
		/// </summary>
		public readonly int arg2;
	}

	public class Sheet1DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

	public class Sheet2DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

	public class Sheet3DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

	public class Sheet4DB:DbBase
	{
		/// <summary>
		/// 字段2
		/// </summary>
		public readonly string bbb1;
		/// <summary>
		/// 字段3
		/// </summary>
		public readonly List<int> bbb2=new List<int>();
		/// <summary>
		/// 字段4
		/// </summary>
		public readonly List<string> bbb3=new List<string>();
		/// <summary>
		/// 字段5
		/// </summary>
		public readonly List<string> bbb4=new List<string>();
		/// <summary>
		/// 字段6
		/// </summary>
		public readonly List<int> bbb5=new List<int>();
	}

}