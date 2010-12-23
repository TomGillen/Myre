using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.UI
{
    /// <summary>
    /// Represents an area which can be anchored to other Frames.
    /// </summary>
    public class Frame
    {
        GraphicsDevice device;
        bool respectSafeArea;
        Rectangle area;
        Vector2 size;
        Points anchoredPoints;
        List<Frame> anchorChildren = new List<Frame>();
        Dictionary<Points, Anchor> anchors = new Dictionary<Points, Anchor>()
        {
            { Points.Left,   new Anchor() },
            { Points.Right,  new Anchor() },
            { Points.Top,    new Anchor() },
            { Points.Bottom, new Anchor() },
            { Points.Centre, new Anchor() }
        };

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Frame Parent { get; private set; }

        /// <summary>
        /// Gets a value indicating whether both the left and right sides of this frame have been anchored.
        /// i.e. Determines if setting the width through SetSize will change the Area; or if the width has been overriden by anchors.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the width of this frame has been fixed by its' anchors; otherwise, <c>false</c>.
        /// </value>
        public bool IsWidthFixed
        {
            get
            {
                return anchoredPoints.Selected(Points.Left) && anchoredPoints.Selected(Points.Right);
            }
        }

        /// <summary>
        /// Gets a value indicating whether both the top and bottom sides of this frame have been anchored.
        /// i.e. Determines if setting the height through SetSize will change the Area; or if the height has been overriden by anchors.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the height of this frame has been fixed by its' anchors; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeightFixed
        {
            get
            {
                return anchoredPoints.Selected(Points.Top) && anchoredPoints.Selected(Points.Bottom);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the positions given are clamped to the safe area.
        /// </summary>
        /// <value><c>true</c> if positions are clamped to the safe area; otherwise, <c>false</c>.</value>
        public bool RespectSafeArea
        {
            get { return respectSafeArea; }
            set
            {
                if (respectSafeArea != value)
                {
                    respectSafeArea = value;
                    UpdateAnchors();
                }
            }
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <value>The area.</value>
        public Rectangle Area
        {
            get { return area; }
            set
            {
                if (area != value)
                {
                    area = value;
                    for (int i = 0; i < anchorChildren.Count; i++)
                        anchorChildren[i].UpdateAnchors();

                    OnAreaChanged();
                }
            }
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice Device
        {
            get { return device; }
        }

        /// <summary>
        /// Occurs when area changes.
        /// </summary>
        public event Action<Frame> AreaChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="graphics">The graphics device.</param>
        /// <param name="parent">The parent.</param>
        public Frame(GraphicsDevice graphics, Frame parent)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            this.device = graphics;
            this.Parent = parent;
        }

        /// <summary>
        /// Determines if the specified point lies within this frame.
        /// </summary>
        /// <param name="point"></param>
        /// <returns><c>true</c> if the point lies within this frame; else <c>false</c>.</returns>
        public virtual bool Contains(Vector2 point)
        {
            return Area.Contains(new Point((int)point.X, (int)point.Y));
        }

        /// <summary>
        /// Sets the size. SetPoint overrides the size set here.
        /// </summary>
        /// <param name="size">The size.</param>
        public void SetSize(Int2D size)
        {
            SetSize(size.X, size.Y);
        }

        /// <summary>
        /// Sets the size. SetPoint overrides the size set here.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void SetSize(int width, int height)
        {
            size = new Vector2(width, height);
            UpdateAnchors();
        }

        /// <summary>
        /// Sets the the specified point on this frame to be at the same position
        /// as the corresponding point on the parent frame.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        public void SetPoint(Points point, int x, int y)
        {
            SetPoint(point, x, y, null, point);
        }

        /// <summary>
        /// Sets the the specified point on this frame to be at the same position
        /// as the corresponding point on the parent frame.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="offset">The offset.</param>
        public void SetPoint(Points point, Int2D offset)
        {
            SetPoint(point, offset.X, offset.Y);
        }

        /// <summary>
        /// Sets the the specified point on this frame to be at the same position
        /// as the corresponding point on the parent frame.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="anchorFrame">The frame this frame is anchored to.</param>
        /// <param name="anchoredTo">The point on the anchorFrame to anchor to.</param>
        public void SetPoint(Points point, Int2D offset, Frame anchorFrame, Points anchoredTo)
        {
            SetPoint(point, offset.X, offset.Y, anchorFrame, anchoredTo);
        }

        /// <summary>
        /// Sets the the specified point on this frame to be at the same position
        /// as the corresponding point on the parent frame.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <param name="anchorFrame">The frame this frame is anchored to.</param>
        /// <param name="anchoredTo">The point on the anchorFrame to anchor to.</param>
        public void SetPoint(Points point, int x, int y, Frame anchorFrame, Points anchoredTo)
        {
            foreach (var side in anchors)
            {
                if (point.Selected(side.Key))
                {
                    Anchor anchor = side.Value;
                    anchor.Start = side.Key;
                    anchor.End = anchoredTo;
                    anchor.AnchorControl = anchorFrame;
                    anchor.Offset = new Vector2(x, y);
                }
            }

            anchoredPoints |= point;

            Frame parent = anchorFrame ?? this.Parent;
            if (parent != null && !parent.anchorChildren.Contains(this))
                parent.anchorChildren.Add(this);

            UpdateAnchors();
        }

        /// <summary>
        /// Clears all anchor points.
        /// </summary>
        public void ClearAllPoints()
        {
            foreach (var anchor in anchors)
            {
                Frame parent = anchor.Value.AnchorControl ?? Parent;
                if (parent != null)
                    parent.anchorChildren.Remove(this);
            }

            anchoredPoints = 0;
            UpdateAnchors();
        }

        private void UpdateAnchors()
        {
            Rectangle parentArea;
            var viewport = device.Viewport;
            if (Parent != null)
                parentArea = Parent.Area;
            else if (RespectSafeArea)
                parentArea = viewport.TitleSafeArea;
            else
                parentArea = new Rectangle(0, 0, viewport.Width, viewport.Height);

            ControlArea area = new ControlArea(0, 0, (int)size.X, (int)size.Y);
            foreach (var side in anchors)
            {
                if (anchoredPoints.Selected(side.Key))
                    side.Value.Apply(ref area, parentArea, anchoredPoints);
            }

            Area = area.ToRectangle();
        }

        /// <summary>
        /// Called when Area changes.
        /// </summary>
        protected void OnAreaChanged()
        {
            if (AreaChanged != null)
                AreaChanged(this);
        }
    }
}
