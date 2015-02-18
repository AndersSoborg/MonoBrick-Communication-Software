using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Graphics.Drawables; 

namespace OmniBotRemote
{
		public class MainActivity : Activity
		{
				AnimationDrawable circleAnimation;
				protected override void OnCreate (Bundle bundle)
				{
						base.OnCreate (bundle);
						SetContentView (Resource.Layout.Main);
						ImageView circle =  (ImageView) FindViewById<ImageView> (Resource.Id.circle);
						circleAnimation = (AnimationDrawable) circle.Drawable;
						circle.Touch += delegate(object sender, View.TouchEventArgs e) {
								if(e.Event.Action == MotionEventActions.Down){
									Console.WriteLine("Circle was pressed");
									circleAnimation.Stop();
									circle.SetImageResource(Resource.Drawable.circle);
								}
								if(e.Event.Action == MotionEventActions.Up){
									Console.WriteLine("Circle is not pressed");
									circle.SetImageDrawable(circleAnimation);
									circleAnimation.Run();
									//circleAnimation.Start();
								}
						};
						//SetContentView(new CircleView(this));
				}
				
				public override void OnWindowFocusChanged (bool hasFocus)
				{
					base.OnWindowFocusChanged (hasFocus);
						if (hasFocus) {
							circleAnimation.Start ();
						}
				}

		}
		
		
		
		class CircleView:SurfaceView{
			
			private ISurfaceHolder surfaceHolder;
			private Paint paint = new Paint();
			private Point point = new Point();
			
			public CircleView(Context context):base(context) {
				surfaceHolder = this.Holder; 
				paint.Color = Color.Red;
			}
			
		/*protected override void OnDraw (Canvas canvas)
		{
			base.OnDraw (canvas);
			paint.SetStyle(Paint.Style.Stroke);
			canvas.DrawCircle(point.X, point.Y, 50, paint);
		}*/

			
			public override bool OnTouchEvent (MotionEvent e)
			{
				if(e.Action == MotionEventActions.Move) {
					if (surfaceHolder.Surface.IsValid) {
						point.X = (int)e.GetX();
						point.Y = (int)e.GetY();
						Console.WriteLine("X " + point.X + " Y " + point.Y);

						//Invalidate();
						//return true;
						Canvas canvas = surfaceHolder.LockCanvas();
						canvas.DrawColor(Color.Black);
						canvas.DrawCircle(point.X, point.Y, 60, paint);
						surfaceHolder.UnlockCanvasAndPost(canvas);
					}
				}
				if(e.Action == MotionEventActions.Up) {
					if (surfaceHolder.Surface.IsValid) {
						Canvas canvas = surfaceHolder.LockCanvas();
						canvas.DrawColor(Color.Black);
						//canvas.DrawCircle(point.X, point.Y, 100, paint);
						surfaceHolder.UnlockCanvasAndPost(canvas);
					}
				}
				return true; 
			}
		}

}


