using System;
using amulware.Graphics;
using OpenTK;
using yatl.Rendering;
using yatl.Utilities;

namespace yatl.Environment
{
    sealed class Wisp : Unit
    {
        private readonly ControlScheme controls;
        private readonly ParticleCloud particles;

        private float health;
        private float healStartTime;

        public Wisp(GameState game, Vector2 position)
            : base(game, position, Settings.Game.Wisp.FrictionCoefficient)
        {
            this.controls = new ControlScheme();

            this.health = Settings.Game.Wisp.MaxHealth;
            this.healStartTime = -1000;

            this.particles = new ParticleCloud(game, 50, this,
                Color.LightYellow.WithAlpha(0.5f) * GlobalRandom.NextFloat(0.03f, 0.1f), 10, 0.6f);
        }

        public float HealthPercentage
        {
            get { return this.health / Settings.Game.Wisp.MaxHealth; }
        }

        public override void Update(GameUpdateEventArgs e)
        {
            var acceleration = this.game.State == GameState.GameOverState.Undetermined
                ? new Vector2(
                this.controls.Right.AnalogAmount - this.controls.Left.AnalogAmount,
                this.controls.Up.AnalogAmount - this.controls.Down.AnalogAmount
                )
                : Vector2.Zero;

            var a = acceleration.Length;
            if (a > 0)
                acceleration /= a;

            this.velocity += acceleration * Settings.Game.Wisp.Acceleration * e.ElapsedTimeF;

            base.Update(e);

            this.particles.Update(e);

            if (this.game.State != GameState.GameOverState.Lost && this.healStartTime <= this.game.Time)
            {
                var healSpeed = this.Tile.Info.Lightness > Settings.Game.Level.LightnessThreshold
                    ? Settings.Game.Wisp.LightHealSpeed
                    : Settings.Game.Wisp.HealSpeed;
                this.health = Math.Min(Settings.Game.Wisp.MaxHealth, this.health + e.ElapsedTimeF * healSpeed);
            }

            if (this.game.State != GameState.GameOverState.Undetermined)
                return;

            if (this.health <= 0)
            {
                this.game.GameOver(false);
            }
            else if (this.Tile.Radius == this.game.Level.Tilemap.Radius)
            {
                this.game.GameOver(true);
            }
        }

        public void Damage(float damage, Vector2 direction)
        {
            var healthPercentageBefore = this.HealthPercentage;

            this.health = Math.Max(0, this.health - damage);
            this.healStartTime = this.game.Time + Settings.Game.Wisp.HealDelay;

            var healthPercentageDifference = healthPercentageBefore - this.HealthPercentage;

            if (healthPercentageDifference > 0.2f)
                this.particles.Explode(healthPercentageDifference, direction.WithZ(0) * 5, 5);

            this.game.Statistics.TotalDamageTaken += healthPercentageDifference;
        }

        public override void Draw(SpriteManager sprites)
        {
            if (this.game.DrawDebug)
            {
                var v = this.game.Level.GetPosition(this.Tile);

                var geo = sprites.EmptyHexagon;
                geo.Color = new Color(Color.Green, 0);
                geo.DrawSprite(v, 0, Settings.Game.Level.HexagonDiameter);
            }

            this.particles.Draw(sprites);

            var healthPercentage = this.HealthPercentage;

            var light = GlobalRandom.NextFloat(healthPercentage * healthPercentage, healthPercentage);

            sprites.PointLight.Draw(this.position.WithZ(1.5f), Color.LightYellow, 1f, 5 + 10 * light);


            var blink = sprites.Blink;

            blink.Color = Color.LightYellow.WithAlpha(0.5f) * 0.3f;
            blink.DrawSprite(this.position.WithZ(1.5f), 0, 1 + light * 2);
        }
    }
}
