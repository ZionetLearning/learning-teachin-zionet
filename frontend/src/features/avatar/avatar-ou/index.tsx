import { useState } from "react";
import Lottie from "react-lottie";
import { Play, Square, Volume2, VolumeX } from "lucide-react";
import { useTTS } from "./hooks";
import { useStyles } from "./style";
import speakingSantaAnimation from "./animations/speakingSantaAnimation.json";
import idleSantaAnimation from "./animations/idleSantaAnimation.json";

export const AvatarOu = () => {
  const classes = useStyles();
  const [text, setText] = useState("שלום, איך שלומך היום?");
  const { speak, stop, toggleMute, isPlaying, isMuted } = useTTS({
    lang: "he-IL",
    rate: 0.9,
    pitch: 1.1,
  });

  const handleSpeak = () => {
    if (isPlaying) {
      stop();
    } else {
      speak(text);
    }
  };

  // Sample Hebrew texts for quick testing
  const sampleTexts = [
    "שלום, איך שלומך היום?",
    "אני בוט מדבר בעברית",
    "טוב לראות אותך פה!",
    "איך אני נשמע לך?",
    "זה הדמו של האווטר המדבר",
  ];

  const animationOptions = {
    loop: true,
    autoplay: true,
    animationData: isPlaying ? speakingSantaAnimation : idleSantaAnimation,
    rendererSettings: { preserveAspectRatio: "xMidYMid slice" },
  };

  return (
    <div className={classes.container}>
      <div className={classes.wrapper}>
        {/* Header */}
        <div className={classes.header}>
          <h1 className={classes.title}>Ou Avatar</h1>
          <p className={classes.subtitle}>אווטר מדבר בעברית עם AI</p>
          <div className={classes.headerDivider}></div>
        </div>

        {/* Main Card */}
        <div className={classes.mainCard}>
          {/* Avatar Section */}
          <div className={classes.avatarSection}>
            <div className={classes.avatarContainer}>
              <div className={classes.avatarWrapper}>
                <div className={classes.avatarGlow}></div>
                <div className={classes.avatarFrame}>
                  <Lottie options={animationOptions} height={250} width={250} />
                </div>
              </div>
            </div>

            {/* Status */}
            <div className={classes.statusContainer}>
              <div
                className={`${classes.statusBadge} ${
                  isPlaying
                    ? classes.statusBadgePlaying
                    : classes.statusBadgeIdle
                }`}
              >
                <div
                  className={`${classes.statusDot} ${
                    isPlaying ? classes.statusDotPlaying : ""
                  }`}
                ></div>
                {isPlaying ? "מדבר עכשיו" : "מוכן לדבר"}
              </div>
            </div>
          </div>

          {/* Controls */}
          <div className={classes.controlsSection}>
            {/* Text Input */}
            <div>
              <label className={classes.inputLabel}>
                <span>הקלד טקסט בעברית:</span>
              </label>
              <div className={classes.textareaWrapper}>
                <textarea
                  value={text}
                  onChange={(e) => setText(e.target.value)}
                  className={classes.textarea}
                  rows={3}
                  placeholder="הקלד כאן את הטקסט שלך..."
                  dir="rtl"
                />
                <div className={classes.charCounter}>{text.length} תווים</div>
              </div>
            </div>

            {/* Sample Texts */}
            <div>
              <label className={classes.inputLabel}>
                <span>דוגמאות:</span>
              </label>
              <div className={classes.samplesGrid}>
                {sampleTexts.map((sample, index) => (
                  <button
                    key={index}
                    onClick={() => setText(sample)}
                    className={`${classes.sampleButton} ${
                      classes[`sampleButton${index}` as keyof typeof classes]
                    }`}
                  >
                    {sample}
                  </button>
                ))}
              </div>
            </div>

            {/* Control Buttons */}
            <div className={classes.buttonsContainer}>
              <button
                onClick={handleSpeak}
                disabled={!text.trim()}
                className={`${classes.primaryButton} ${
                  isPlaying
                    ? classes.primaryButtonPlaying
                    : classes.primaryButtonIdle
                }`}
              >
                {isPlaying ? (
                  <>
                    <Square size={20} />
                    עצור דיבור
                  </>
                ) : (
                  <>
                    <Play size={20} />
                    התחל דיבור
                  </>
                )}
              </button>

              <button
                onClick={toggleMute}
                className={`${classes.muteButton} ${
                  isMuted ? classes.muteButtonMuted : classes.muteButtonUnmuted
                }`}
              >
                {isMuted ? <VolumeX size={20} /> : <Volume2 size={20} />}
              </button>
            </div>

            {/* Footer */}
            <div className={classes.footer}>
              <p className={classes.footerText}>
                Web Speech API • תמיכה מלאה בעברית
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
