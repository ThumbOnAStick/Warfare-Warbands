using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    public class Window_PolicyTree : Window
    {
        private PolicyTree _policyTree;

        public Window_PolicyTree()
        {
            _policyTree = new PolicyTree();
        }

        public Window_PolicyTree(PolicyTree policyTree)
        {
            _policyTree = policyTree;
        }

        public override void DoWindowContents(Rect inRect)
        {
            float height = inRect.y;
            for(int i = 0; i < _policyTree.Roots.Count; i++)
            {
                Rect rootRect = new Rect(inRect.x, height + PolicyDrawer.margin + PolicyDrawer.policyBoxHeight, PolicyDrawer.policyBoxWidth, PolicyDrawer.policyBoxHeight);
                _policyTree.Roots[i].DrawPolicyAndChildren(rootRect, out height);
            }
        }

    }
}
