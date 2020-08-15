﻿using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
    [ObjectSystem]
    public class UnitComponentSystem: AwakeSystem<UnitComponent>
    {
        public override void Awake(UnitComponent self)
        {
            self.Awake();
        }
    }

    public class UnitComponent: Component
    {
        public static UnitComponent Instance { get; private set; }

        public Unit MyUnit;

        private readonly Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();

        public void Awake()
        {
            Instance = this;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            GameObjectPool gameObjectPool = Game.Scene.GetComponent<GameObjectPool>();
            foreach (Unit unit in this.idUnits.Values)
            {
                gameObjectPool.Recycle(unit);
            }
            gameObjectPool.Recycle(MyUnit);
            this.idUnits.Clear();

            Instance = null;
        }

        public void Add(Unit unit)
        {
            this.idUnits.Add(unit.Id, unit);
            unit.Parent = this;
        }

        public Unit Get(long id)
        {
            Unit unit;
            if (this.idUnits.TryGetValue(id, out unit))
            {
                if (unit.IsDisposed)
                {
                    Log.Error("想获得的Unit已经Dispose了");
                    return null;
                }

                return unit;
            }

            foreach (var VARIABLE in idUnits)
            {
                unit = VARIABLE.Value.GetComponent<ChildrenUnitComponent>().GetUnit(id);
                if (unit != null)
                {
                    return unit;
                }
            }

            Log.Info($"实在没有找到unit，id为{id}");
            return null;
        }

        public void Remove(long id)
        {
            Unit unit;
            this.idUnits.TryGetValue(id, out unit);
            this.idUnits.Remove(id);
            unit?.Dispose();
        }

        public void RemoveNoDispose(long id)
        {
            this.idUnits.Remove(id);
        }

        public int Count
        {
            get
            {
                return this.idUnits.Count;
            }
        }

        public Unit[] GetAll()
        {
            return this.idUnits.Values.ToArray();
        }
    }
}