using System.Collections;
using Isochronous.TestKit;
using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UpgradePlanner.Tests
{
	/// <summary>
	/// PDialog.Draggable, driven the way Unity's input module drives a drag: begin, drag and
	/// end events on the object under the pointer, bubbling up to the handler. Needs the mod
	/// built against a PLib that has the feature (-p:PLibDraggable=true with that package).
	/// </summary>
	public static class DialogDrag
	{
		private const float Tolerance = 1.5f;
		private static readonly Vector2 Delta = new Vector2(120f, -80f);

		[OniTest("PLib: a draggable dialog follows its title bar by the pointer's movement"), Layer(2)]
		public static IEnumerator FollowsTitleBar(TestContext t)
		{
			Dialog d = Open(t, draggable: true);
			yield return t.Frames(2);
			Vector2 before = d.ScreenPosition();
			d.Drag(d.Title, Delta);
			yield return null;
			Vector2 moved = d.ScreenPosition() - before;
			t.Assert.True((moved - Delta).magnitude < Tolerance, "the dialog moved with the pointer (moved " + moved + ", pointer " + Delta + ")");
		}

		[OniTest("PLib: a drag that starts on the title text moves the dialog too"), Layer(2)]
		public static IEnumerator TitleTextBubbles(TestContext t)
		{
			Dialog d = Open(t, draggable: true);
			yield return t.Frames(2);
			Vector2 before = d.ScreenPosition();
			LocText text = d.Title.GetComponentInChildren<LocText>();
			t.Assert.NotNull(text, "the title bar has its text");
			d.Drag(text.gameObject, Delta);
			yield return null;
			t.Assert.True((d.ScreenPosition() - before - Delta).magnitude < Tolerance, "the drag reached the title bar's handler from its text");
		}

		[OniTest("PLib: a dialog that is not draggable stays put"), Layer(2)]
		public static IEnumerator NotDraggableStays(TestContext t)
		{
			Dialog d = Open(t, draggable: false);
			yield return t.Frames(2);
			Vector2 before = d.ScreenPosition();
			d.Drag(d.Title, Delta);
			yield return null;
			t.Assert.True((d.ScreenPosition() - before).magnitude < Tolerance, "the dialog did not move");
		}

		[OniTest("PLib: the right mouse button does not drag a dialog"), Layer(2)]
		public static IEnumerator RightButtonIgnored(TestContext t)
		{
			Dialog d = Open(t, draggable: true);
			yield return t.Frames(2);
			Vector2 before = d.ScreenPosition();
			d.Drag(d.Title, Delta, PointerEventData.InputButton.Right);
			yield return null;
			t.Assert.True((d.ScreenPosition() - before).magnitude < Tolerance, "the dialog did not move");
		}

		[OniTest("PLib: a drag on the dialog's body does not move it"), Layer(2)]
		public static IEnumerator BodyDoesNotDrag(TestContext t)
		{
			Dialog d = Open(t, draggable: true);
			yield return t.Frames(2);
			Vector2 before = d.ScreenPosition();
			d.Drag(d.Body, Delta);
			yield return null;
			t.Assert.True((d.ScreenPosition() - before).magnitude < Tolerance, "the dialog did not move");
		}

		[OniTest("PLib: a dialog dragged off the screen stops at the edge"), Layer(2)]
		public static IEnumerator ClampedToParent(TestContext t)
		{
			Dialog d = Open(t, draggable: true);
			yield return t.Frames(2);
			d.Drag(d.Title, new Vector2(100000f, 100000f));
			yield return null;
			t.Assert.True(d.InsideParent(out string where), "the dialog stays inside its parent after a drag up and right (" + where + ")");
			d.Drag(d.Title, new Vector2(-100000f, -100000f));
			yield return null;
			t.Assert.True(d.InsideParent(out where), "the dialog stays inside its parent after a drag down and left (" + where + ")");
			// Held in the lower left corner, the dialog can only go up and right.
			Vector2 before = d.ScreenPosition();
			Vector2 away = new Vector2(120f, 80f);
			d.Drag(d.Title, away);
			yield return null;
			t.Assert.True((d.ScreenPosition() - before - away).magnitude < Tolerance, "a normal drag works again after being held at the edge");
		}

		[OniTest("PLib: a dragged dialog still closes from its button"), Layer(2)]
		public static IEnumerator ClosesAfterDrag(TestContext t)
		{
			Dialog d = Open(t, draggable: true);
			yield return t.Frames(2);
			d.Drag(d.Title, Delta);
			yield return null;
			KButton close = FindDeep(d.Root.transform, PDialog.DIALOG_KEY_CLOSE)?.GetComponent<KButton>();
			t.Assert.NotNull(close, "the close button is there");
			close.SignalClick(KKeyCode.Mouse0);
			yield return t.Frames(2);
			t.Assert.True(d.Root == null, "the dialog is gone");
			t.Assert.Eq(PDialog.DIALOG_KEY_CLOSE, d.ClosedWith, "the close callback ran with the close key");
		}

		// ---- a dialog under test ----

		private sealed class Dialog
		{
			public GameObject Root;
			public GameObject Title;
			public GameObject Body;
			public string ClosedWith;
			private readonly GraphicRaycaster raycaster;

			public Dialog(GameObject root)
			{
				Root = root;
				Title = FindDeep(root.transform, "Title")?.gameObject;
				Body = FindDeep(root.transform, "Body")?.gameObject;
				Canvas canvas = root.GetComponentInParent<Canvas>();
				raycaster = canvas != null ? canvas.rootCanvas.GetComponent<GraphicRaycaster>() : null;
			}

			private Camera Camera => raycaster != null ? raycaster.eventCamera : null;

			/// <summary>The dialog's position on screen, in pixels.</summary>
			public Vector2 ScreenPosition()
			{
				return RectTransformUtility.WorldToScreenPoint(Camera, Root.transform.position);
			}

			/// <summary>
			/// Begin, drag and end on the object under the pointer, bubbling to the handler as
			/// the standalone input module does, with the pointer starting on the object and
			/// moving by delta in pixels.
			/// </summary>
			public void Drag(GameObject under, Vector2 delta, PointerEventData.InputButton button = PointerEventData.InputButton.Left)
			{
				Vector2 from = RectTransformUtility.WorldToScreenPoint(Camera, under.transform.position);
				// The game has an EventSystem class of its own; Unity's is the one a pointer event takes.
				var data = new PointerEventData(UnityEngine.EventSystems.EventSystem.current)
				{
					button = button,
					position = from,
					pressPosition = from,
					pointerPressRaycast = new RaycastResult { module = raycaster, gameObject = under },
				};
				GameObject handler = ExecuteEvents.GetEventHandler<IBeginDragHandler>(under) ?? under;
				ExecuteEvents.Execute(handler, data, ExecuteEvents.beginDragHandler);
				data.position = from + delta;
				data.delta = delta;
				ExecuteEvents.Execute(handler, data, ExecuteEvents.dragHandler);
				ExecuteEvents.Execute(handler, data, ExecuteEvents.endDragHandler);
			}

			/// <summary>Every corner of the dialog lies within its parent's rectangle (a pixel of slack).</summary>
			public bool InsideParent(out string where)
			{
				var rt = Root.transform as RectTransform;
				var parent = rt.parent as RectTransform;
				var corners = new Vector3[4];
				rt.GetWorldCorners(corners);
				Rect inside = parent.rect;
				foreach (Vector3 corner in corners)
				{
					Vector3 local = parent.InverseTransformPoint(corner);
					if (local.x < inside.xMin - 1f || local.x > inside.xMax + 1f || local.y < inside.yMin - 1f || local.y > inside.yMax + 1f)
					{
						where = "corner at " + local + " outside " + inside;
						return false;
					}
				}
				where = "all four corners inside " + inside;
				return true;
			}
		}

		private static Dialog Open(TestContext t, bool draggable)
		{
			Dialog holder = null;
			var dialog = new PDialog("DragTest")
			{
				Title = "Drag test",
				Size = new Vector2(420f, 260f),
				Parent = PDialog.GetParentObject(),
				SortKey = 300f,
#if PLIB_DRAGGABLE
				Draggable = draggable,
#endif
				DialogClosed = key => { if (holder != null) { holder.ClosedWith = key; holder.Root = null; } },
			};
#if !PLIB_DRAGGABLE
			if (draggable)
				t.Skip("built without PLIB_DRAGGABLE: no PDialog.Draggable in this PLib");
#endif
			dialog.Body.AddChild(new PLabel("Body") { Text = "Drag me by my title bar", FlexSize = Vector2.one });
			GameObject root = dialog.Build();
			holder = new Dialog(root);
			KScreen screen = root.GetComponent<KScreen>();
			screen.Activate();
			t.Cleanup(() =>
			{
				if (holder.Root != null)
					holder.Root.GetComponent<KScreen>().Deactivate();
			});
			t.Log("dialog at " + holder.ScreenPosition() + " on screen " + Screen.width + "x" + Screen.height + (draggable ? ", draggable" : ""));
			return holder;
		}

		private static Transform FindDeep(Transform root, string name)
		{
			if (root == null)
				return null;
			if (root.name == name)
				return root;
			for (int i = 0; i < root.childCount; i++)
			{
				Transform found = FindDeep(root.GetChild(i), name);
				if (found != null)
					return found;
			}
			return null;
		}
	}
}
