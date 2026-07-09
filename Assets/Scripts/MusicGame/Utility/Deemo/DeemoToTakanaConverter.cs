#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime;

namespace MusicGame.Utility.Deemo
{
    public static class DeemoToTakanaConverter
    {
        private const float PositionScale = 5f / 3f;

        public static ChartInfo Convert(DeemoChart deemoChart)
        {
            var slideNotes = new HashSet<DeemoNote>();
            foreach (var link in deemoChart.Links)
            {
                foreach (var note in link.Notes)
                    slideNotes.Add(note);
            }

            var components = new List<IChartModel>();

            foreach (var note in deemoChart.Notes)
            {
                var timeJudge = new T3Time(note.Time);
                var position = note.Position * PositionScale;
                var width = note.Size * PositionScale;

                if (slideNotes.Contains(note))
                {
                    components.Add(new DraftHit(timeJudge, HitType.Slide, position, width));
                }
                else if (note.IsSwipe)
                {
                    components.Add(new DraftHit(timeJudge, HitType.Tap, position, width));
                }
                else if (note.Duration > 0f)
                {
                    var timeEnd = new T3Time(note.Time + note.Duration);
                    components.Add(new DraftHold(timeJudge, timeEnd, position, width));
                }
                else
                {
                    components.Add(new DraftHit(timeJudge, HitType.Tap, position, width));
                }
            }

            return new ChartInfo(components);
        }
    }
}
