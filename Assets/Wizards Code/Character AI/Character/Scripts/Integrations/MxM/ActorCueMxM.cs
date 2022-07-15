#if MXM_PRESENT
using MxM;
using System.Collections;
using UnityEngine;
using WizardsCode.Character;

namespace WizardsCode.MxMExtensions
{
    [CreateAssetMenu(fileName = "New MxM ActorCue", menuName = "Wizards Code/Actor/MxM Cue")]
    public class ActorCueMxM : ActorCue
    {

        [Header("MxM Configuration")]
        [SerializeField, Tooltip("The name of the MxM Calibration to set upon this cue being prompted.")]
        string m_calibrationName;
        [SerializeField, Tooltip("The trajectory generator configuration to use when this cue is active.")]
        MxMActorTrajectoryGeneratorConfig m_TrajectoryConfig;
        [SerializeField, Tooltip("The Warping configuration to use when this cue is active.")]
        WarpModule m_WarpingConfig;

        [Header("MxM Events and Tags")]
        [SerializeField, Tooltip("The MxM event to fire when this cue is prompted.")]
        internal MxMEventDefinition m_MxMEvent;
        [SerializeField, Tooltip("Remove all required tags?")]
        bool m_RemoveAllRequiredTags = false;
        [SerializeField, Tooltip("If there is a required tag set do we need to add it (set to true) or remove it (set to false)?")]
        bool m_AddRequiredTag = true;
        [SerializeField, Tooltip("The MxM required tag to add or remove.")]
        string m_RequiredTag;
        [SerializeField, Tooltip("Remove all favored tags?")]
        bool m_RemoveAllFavouredTags = false;
        [SerializeField, Tooltip("If there is a favoured tag set do we need to add it (set to true) or remove it (set to false)?")]
        bool m_AddFavouredTag = true;
        [SerializeField, Tooltip("The MxM favoured tag to set.")]
        string m_FavourTag;
        [SerializeField, Tooltip("The multiplier to use for the favour tags. The lower this multiplier the more likely an animation with this tag will be used.")]
        float m_favourMultiplier = 0.6f;

        private MxMAnimator m_mxmAnimator;
        private MxMActorTrajectoryGenerator trajectoryGenerator;
        private ETags requiredEtag;
        private ETags favouredEtag;

        public EventContact[] contactPoints {
            get; set;
        }

        public override IEnumerator Prompt(BaseActorController actor)
        {
            //TODO OPTIMIZATION cache the animator and trajectory generator
            m_mxmAnimator = actor.GetComponent<MxMAnimator>();
            trajectoryGenerator = actor.GetComponent<MxMActorTrajectoryGenerator>();

            if (m_RemoveAllFavouredTags)
            {
                m_mxmAnimator.ClearFavourTags();
            }

            //TODO OPTIMIZATION don't convert from string to etag every time prompted
            if (!string.IsNullOrEmpty(m_FavourTag))
            {
                favouredEtag = m_mxmAnimator.CurrentAnimData.FavourTagFromName(m_FavourTag);
                if (m_AddFavouredTag)
                {
                    m_mxmAnimator.AddFavourTags(favouredEtag);
                } else
                {
                    m_mxmAnimator.RemoveFavourTags(favouredEtag);
                }
                m_mxmAnimator.SetFavourMultiplier(m_favourMultiplier);
            }

            if (m_RemoveAllRequiredTags)
            {
                m_mxmAnimator.ClearAllTags();
            }

            if (!string.IsNullOrEmpty(m_RequiredTag))
            {
                requiredEtag = m_mxmAnimator.CurrentAnimData.TagFromName(m_RequiredTag);
                if (m_AddRequiredTag)
                {
                    m_mxmAnimator.AddRequiredTags(requiredEtag);
                } else
                {
                    m_mxmAnimator.RemoveRequiredTags(requiredEtag);
                }
            }

            if (!string.IsNullOrEmpty(m_calibrationName))
            {
                //TODO OPTIMIZATION don't use string names to look up the calibration
                m_mxmAnimator.SetCalibrationData(m_calibrationName);
            }

            if (m_TrajectoryConfig != null)
            {
                trajectoryGenerator.SetTrajectoryConfig(m_TrajectoryConfig);
            }

            if (m_WarpingConfig != null) {
                m_mxmAnimator.SetWarpOverride(m_WarpingConfig);
            }

            if (m_MxMEvent != null)
            {
                m_MxMEvent.ClearContacts();
                for (int i = 0; i < contactPoints.Length; i++)
                {
                    m_MxMEvent.AddEventContact(contactPoints[i]);
                }
                m_mxmAnimator.BeginEvent(m_MxMEvent);
            }

            return base.Prompt(actor);
        }
    }
}
#endif