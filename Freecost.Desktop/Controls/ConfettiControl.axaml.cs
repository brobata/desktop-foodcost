using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace Freecost.Desktop.Controls;

public partial class ConfettiControl : UserControl
{
    private readonly List<ConfettiParticle> _particles = new();
    private DispatcherTimer? _animationTimer;
    private readonly Random _random = new();

    private static readonly Color[] ConfettiColors =
    {
        Color.FromRgb(122, 181, 29),  // Brand green #7AB51D
        Color.FromRgb(139, 201, 51),  // Light green #8BC933
        Color.FromRgb(255, 215, 0),   // Gold #FFD700
        Color.FromRgb(255, 99, 71),   // Tomato
        Color.FromRgb(64, 224, 208),  // Turquoise
        Color.FromRgb(255, 105, 180), // Hot pink
        Color.FromRgb(138, 43, 226),  // Blue violet
        Color.FromRgb(255, 140, 0)    // Dark orange
    };

    public ConfettiControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Start the confetti celebration animation
    /// </summary>
    public void Celebrate(int particleCount = 100, int durationMs = 3000)
    {
        if (ConfettiCanvas == null) return;

        // Clear any existing particles
        ConfettiCanvas.Children.Clear();
        _particles.Clear();

        // Create particles
        for (int i = 0; i < particleCount; i++)
        {
            CreateParticle();
        }

        // Start animation timer
        _animationTimer?.Stop();
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
        _animationTimer.Start();

        // Auto-stop after duration
        DispatcherTimer.RunOnce(() =>
        {
            _animationTimer?.Stop();
            ConfettiCanvas.Children.Clear();
            _particles.Clear();
        }, TimeSpan.FromMilliseconds(durationMs));
    }

    private void CreateParticle()
    {
        if (ConfettiCanvas == null) return;

        var width = ConfettiCanvas.Bounds.Width > 0 ? ConfettiCanvas.Bounds.Width : 800;
        var color = ConfettiColors[_random.Next(ConfettiColors.Length)];

        // Create particle shape (rectangle or ellipse)
        Shape shape;
        if (_random.Next(2) == 0)
        {
            shape = new Rectangle
            {
                Width = _random.Next(8, 15),
                Height = _random.Next(8, 15),
                Fill = new SolidColorBrush(color)
            };
        }
        else
        {
            shape = new Ellipse
            {
                Width = _random.Next(6, 12),
                Height = _random.Next(6, 12),
                Fill = new SolidColorBrush(color)
            };
        }

        var particle = new ConfettiParticle
        {
            Shape = shape,
            X = _random.NextDouble() * width,
            Y = -20, // Start above the screen
            VelocityX = (_random.NextDouble() - 0.5) * 4,
            VelocityY = _random.NextDouble() * 2 + 2,
            RotationSpeed = (_random.NextDouble() - 0.5) * 10,
            Rotation = 0
        };

        _particles.Add(particle);
        ConfettiCanvas.Children.Add(shape);

        Canvas.SetLeft(shape, particle.X);
        Canvas.SetTop(shape, particle.Y);
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (ConfettiCanvas == null) return;

        var height = ConfettiCanvas.Bounds.Height > 0 ? ConfettiCanvas.Bounds.Height : 600;

        foreach (var particle in _particles)
        {
            // Update physics
            particle.VelocityY += 0.2; // Gravity
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Rotation += particle.RotationSpeed;

            // Update position
            Canvas.SetLeft(particle.Shape, particle.X);
            Canvas.SetTop(particle.Shape, particle.Y);

            // Apply rotation
            particle.Shape.RenderTransform = new RotateTransform(particle.Rotation);

            // Hide particles that fell off screen
            if (particle.Y > height)
            {
                particle.Shape.IsVisible = false;
            }
        }
    }

    private class ConfettiParticle
    {
        public Shape Shape { get; set; } = null!;
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Rotation { get; set; }
        public double RotationSpeed { get; set; }
    }
}
