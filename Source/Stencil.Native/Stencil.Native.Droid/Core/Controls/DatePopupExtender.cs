
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Widget;
using Android.App;
using Android.Support.V4.App;
using BetterPickers.CalendarDatePickers;
using BetterPickers.DatePickers;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    public class DatePopupExtender : BaseClass, IDisposable
    {

        public DatePopupExtender(FragmentActivity activity, TextView textView, Action<DateTime> dateSelectedAction, Action<bool> dateFocusAction)
            : base("DatePopupExtender")
        {
            this.Activity = activity;
            this.DateSelectedAction = dateSelectedAction;
            this.FocusAction = dateFocusAction;
            this.TextView = textView;

            this.TextView.Focusable = true;
            this.TextView.FocusableInTouchMode = true;
            EditText editText = textView as EditText;
            if (editText != null)
            {
                editText.Touch += EditText_Touch;
                editText.FocusChange += EditText_FocusChange;
            }
            else
            {
                this.TextView.Click += View_Click;
                this.TextView.FocusChange += TextView_FocusChange;
            }
        }




        protected FragmentActivity Activity { get; set; }
        protected TextView TextView { get; set; }
        protected Action<DateTime> DateSelectedAction { get; set; }
        protected Action<bool> FocusAction { get; set; }
        public DateTime? LastPicked { get; set; }

        public bool PickTime { get; set; }
        public bool UseCalendar { get; set; }

        private bool _showing;
        private DateSetHandler _handler;


        protected void EditText_Touch (object sender, Android.Views.View.TouchEventArgs e)
        {
            if (e.Event.Action == Android.Views.MotionEventActions.Up)
            {
                this.OnItemClicked();
            }
        }
        protected virtual void View_Click(object sender, EventArgs e)
        {
            this.OnItemClicked();
        }
        public virtual void OnItemClicked()
        {
            base.ExecuteMethod("OnItemClicked", delegate()
            {
                _showing = true;
                DateTime parsed = DateTime.UtcNow;
                if(!DateTime.TryParse(this.TextView.Text, out parsed))
                {
                    parsed = DateTime.UtcNow;
                }

                if(FocusAction != null)
                {
                    FocusAction(true);
                }

                if(this.PickTime)
                {
                    var picker = new BetterPickers.RadialTimePickers.RadialTimePickerDialog();
                    picker.SetThemeCustom(Resource.Style.BetterPickersRadialTimePickerDialog);
                    picker.SetStartTime(DateTime.Now.Hour, DateTime.Now.Minute);
                    picker.SetDoneText("Done");

                    picker.TimeSet += picker_TimeSet;
                    picker.Show(this.Activity.SupportFragmentManager, null);
                }
                else
                {
                    if(this.UseCalendar)
                    {
                        _handler = new DateSetHandler()
                        {
                            Action = this.OnDateTimePicked
                        };
                        CalendarDatePickerDialog picker = CalendarDatePickerDialog.NewInstance(_handler, parsed.Year, parsed.Month, parsed.Day);
                        picker.Show(this.Activity.SupportFragmentManager, "");
                    }
                    else
                    {
                        var picker = new DatePickerBuilder()
                            .SetFragmentManager(this.Activity.SupportFragmentManager)
                            .SetStyleResId(Resource.Style.BetterPickersDialogFragment);

                        picker.AddDatePickerDialogHandler(OnDateTimePicked);

                        picker.Show();
                    }


                }
            });
        }

        protected void dailog_CancelEvent (object sender, EventArgs e)
        {
            base.ExecuteMethod("dailog_CancelEvent", delegate()
            {
                if(FocusAction != null)
                {
                    FocusAction(false);
                }
            });
        }

        protected void picker_TimeSet(object sender, BetterPickers.RadialTimePickers.RadialTimePickerDialog.TimeSetEventArgs e)
        {
            base.ExecuteMethod("picker_TimeSet", delegate()
            {
                DateTime updated = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, e.P1, e.P2, 0, 0, DateTimeKind.Local);
                this.LastPicked = updated;
                if (DateSelectedAction != null)
                {
                    DateSelectedAction(updated);
                }
                if(FocusAction != null)
                {
                    FocusAction(false);
                }
            });
        }
        protected void OnDateTimePicked(int reference, int year, int month, int day)
        {
            base.ExecuteMethod("OnDateTimePicked", delegate()
            {
                _showing = false;
                // android months are zero based
                DateTime updated = new DateTime(year, month + 1, day, 0, 0, 0, 0, DateTimeKind.Utc);
                this.LastPicked = updated;
                if (DateSelectedAction != null)
                {
                    DateSelectedAction(updated);
                }
                if(FocusAction != null)
                {
                    FocusAction(false);
                }
            });
        }
        private void TextView_FocusChange (object sender, Android.Views.View.FocusChangeEventArgs e)
        {
            base.ExecuteMethod("TextView_FocusChange", delegate()
            {
                if(e.HasFocus)
                {
                    if(!_showing)
                    {
                        this.OnItemClicked();
                    }
                }
            });
        }

        private void EditText_FocusChange (object sender, Android.Views.View.FocusChangeEventArgs e)
        {
            base.ExecuteMethod("EditText_FocusChange", delegate()
            {
                if(e.HasFocus)
                {
                    if(!_showing)
                    {
                        this.OnItemClicked();
                    }
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (this.TextView != null)
                {
                    EditText editText = this.TextView as EditText;
                    if (editText != null)
                    {
                        editText.Touch -= EditText_Touch;
                        editText.FocusChange -= EditText_FocusChange;
                    }
                    else
                    {
                        this.TextView.Click -= View_Click;
                        this.TextView.FocusChange -= TextView_FocusChange;
                    }
                }
                this.TextView = null;
                this.DateSelectedAction = null;
                this.FocusAction = null;
                this.Activity = null;
            }
            base.Dispose(disposing);
        }

        public class DateSetHandler : Java.Lang.Object, BetterPickers.CalendarDatePickers.CalendarDatePickerDialog.IOnDateSetListener
        {
            public Action<int, int, int, int> Action { get; set; }
            public void OnDateSet(CalendarDatePickerDialog p0, int p1, int p2, int p3)
            {
                if (this.Action != null)
                {
                    this.Action(0, p1, p2, p3);
                }
            }

        }
    }
}

