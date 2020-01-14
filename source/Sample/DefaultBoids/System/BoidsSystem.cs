﻿using System;
using System.Collections.Generic;
using DefaultBoids.Component;
using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;
using Microsoft.Xna.Framework;

namespace DefaultBoids.System
{
    [With(typeof(DrawInfo), typeof(Acceleration), typeof(Velocity), typeof(Grid))]
    public sealed class BoidsSystem : AEntitySystem<float>
    {
        private readonly float _maxDistance;
        private readonly float _maxDistanceSquared;
        private readonly World _world;
        private readonly Grid _grid;

        public BoidsSystem(World world, IParallelRunner runner, Grid grid)
            : base(world, runner)
        {
            _maxDistance = DefaultGame.NeighborRange;
            _maxDistanceSquared = MathF.Pow(_maxDistance, 2);
            _world = world;
            _grid = grid;
        }

        protected override void Update(float state, ReadOnlySpan<Entity> entities)
        {
            using Components<DrawInfo> drawInfos = _world.GetComponents<DrawInfo>();
            using Components<Velocity> velocities = _world.GetComponents<Velocity>();
            using Components<Acceleration> accelerations = _world.GetComponents<Acceleration>();

            foreach (ref readonly Entity entity in entities)
            {
                Vector2 position = entity.Get(drawInfos).Position;
                Vector2 separation = Vector2.Zero;
                Vector2 alignment = Vector2.Zero;
                Vector2 cohesion = Vector2.Zero;
                int neighborCount = 0;

                foreach (List<Entity> neighbors in _grid.GetEnumerator(position))
                {
                    foreach (Entity neighbor in neighbors)
                    {
                        if (entity == neighbor)
                        {
                            continue;
                        }

                        Vector2 otherPosition = neighbor.Get(drawInfos).Position;

                        Vector2 offset = position - otherPosition;

                        if (offset.LengthSquared() < _maxDistanceSquared)
                        {
                            separation += Vector2.Normalize(offset);

                            alignment += neighbor.Get(velocities).Value;

                            cohesion += otherPosition;

                            ++neighborCount;
                        }
                    }
                }

                if (neighborCount > 0)
                {
                    alignment = (alignment / neighborCount) - entity.Get(velocities).Value;

                    cohesion = position - (cohesion / neighborCount);
                }

                entity.Get(accelerations).Value =
                    (separation * DefaultGame.BehaviorSeparationWeight)
                    + (alignment * DefaultGame.BehaviorAlignmentWeight)
                    + (cohesion * DefaultGame.BehaviorCohesionWeight);
            }
        }
    }
}
