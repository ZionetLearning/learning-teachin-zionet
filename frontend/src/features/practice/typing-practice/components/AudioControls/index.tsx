import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

interface AudioState {
  isPlaying: boolean;
  hasPlayed: boolean;
  error: string | null;
}

interface AudioControlsProps {
  phase: "ready" | "playing" | "typing" | "feedback";
  audioState: AudioState;
  onPlayAudio: () => void;
  onReplayAudio: () => void;
}

export const AudioControls = ({
  phase,
  audioState,
  onPlayAudio,
  onReplayAudio,
}: AudioControlsProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  return (
    <div className={classes.audioControls}>
      <div className={classes.audioInfo}>
        <div className={classes.audioIcon}>
          {audioState.isPlaying ? "🔊" : "🎧"}
        </div>
        <div className={classes.audioText}>
          {phase === "ready" && t('pages.typingPractice.clickPlayToHearTheHebrewText')}
          {phase === "playing" && t('pages.typingPractice.playingAudio')}
          {phase === "typing" && t('pages.typingPractice.typeWhatYouHeard')}
        </div>
      </div>

      <div className={classes.audioButtons}>
        {phase === "ready" && (
          <button
            className={classes.playButton}
            onClick={onPlayAudio}
            disabled={audioState.isPlaying}
          >
            {audioState.isPlaying ? (
              <>
                <span className={classes.loadingSpinner} />
                {t('pages.typingPractice.playing')}
              </>
            ) : (
              <>{t('pages.typingPractice.playingAudio')}</>
            )}
          </button>
        )}

        {(phase === "typing" || audioState.hasPlayed) && (
          <button
            className={classes.replayButton}
            onClick={onReplayAudio}
            disabled={audioState.isPlaying}
          >
            {audioState.isPlaying ? (
              <>
                <span className={classes.loadingSpinner} />
                {t('pages.typingPractice.playing')}
              </>
            ) : (
              <>{t('pages.typingPractice.replay')}</>
            )}
          </button>
        )}
      </div>

      {audioState.error && (
        <div className={classes.audioError}>
          <div>⚠️</div>
          <div className={classes.audioErrorText}>{audioState.error}</div>
          <button
            className={classes.audioRetryButton}
            onClick={onPlayAudio}
            disabled={audioState.isPlaying}
          >
            {t('pages.typingPractice.tryAgain')}n
          </button>
        </div>
      )}
    </div>
  );
};
