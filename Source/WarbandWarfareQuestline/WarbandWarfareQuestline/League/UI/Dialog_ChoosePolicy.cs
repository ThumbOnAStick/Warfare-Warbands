using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.UI
{
    public class ChoiceLetter_ChoosePawn_ChoosePolicy : ChoiceLetter
    {
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                yield return new DiaOption() { };
                yield return new DiaOption();
            }
        }
    }
}
