using UnityEngine;

namespace JZWL
{
    public class GreyColorGroup : MonoBehaviour
    {
        private GreyColor[] m_greyColors = null;
        private GreyColor[] m_greyColor = null;

        public void SetGrey()
        {
            m_greyColor = GetComponents<GreyColor>();
            m_greyColors = GetComponentsInChildren<GreyColor>(true);
            ArrayUtils.ForEach(ref m_greyColor, (GreyColor child) =>
            {
                child.SetGrey();
            });
            ArrayUtils.ForEach(ref m_greyColors, (GreyColor child) =>
            {
                child.SetGrey();
            });
        }

        public void Reset()
        {
            ArrayUtils.ForEach(ref m_greyColor, (GreyColor child) =>
            {
                child.Reset();
            });
            ArrayUtils.ForEach(ref m_greyColors, (GreyColor child) =>
            {
                child.Reset();
            });
        }
    }
}
