using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;

namespace BouncingBubble
{
    public class Particle
    {
        public Point Position { get; set; }
        public Vector Velocity { get; set; }
        public double Lifespan { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class ParticleEffect : FrameworkElement
    {
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private DispatcherTimer timer;

        public ParticleEffect()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            timer.Tick += UpdateParticles;
            timer.Start();
        }

        public void CreateSpark(Point position)
        {
            for (int i = 0; i < 10; i++) // Generate 10 particles
            {
                particles.Add(new Particle
                {
                    Position = position,
                    Velocity = new Vector(random.NextDouble() * 4 - 2, random.NextDouble() * 4 - 2),
                    Lifespan = 1.0,
                    Color = Brushes.OrangeRed
                });
            }
            InvalidateVisual();
        }

        private void UpdateParticles(object sender, EventArgs e)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Position += particles[i].Velocity;
                particles[i].Lifespan -= 0.05;
                if (particles[i].Lifespan <= 0)
                    particles.RemoveAt(i);
            }
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            foreach (var particle in particles)
            {
                dc.DrawEllipse(particle.Color, null, particle.Position, 2, 2);
            }
        }
    }
}
