import { useState } from "react";
import Lottie from "react-lottie";
import { Play, Square, Volume2, VolumeX } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import speakingSantaAnimation from "./animations/speakingSantaAnimation.json";
import idleSantaAnimation from "./animations/idleSantaAnimation.json";
import { useAvatarSpeech } from "@/hooks";

export const AvatarOu = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [text, setText] = useState("שלום, איך שלומך היום?");
  const { speak, stop, toggleMute, isPlaying, isMuted } = useAvatarSpeech({
    volume: 0.9,
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
          <h1 className={classes.title}>{t("pages.avatarOu.ouAvatar")}</h1>
          <p className={classes.subtitle}>
            {t("pages.avatarOu.avatarSpeaksHebrewWithAi")}
          </p>
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
                  className={`${classes.statusDot} ${isPlaying ? classes.statusDotPlaying : ""}`}
                ></div>
                {isPlaying
                  ? t("pages.avatarOu.speakingNow")
                  : t("pages.avatarOu.readyToSpeak")}
              </div>
            </div>
          </div>

          {/* Controls */}
          <div className={classes.controlsSection}>
            {/* Text Input */}
            <div>
              <label className={classes.inputLabel}>
                <span>{t("pages.avatarOu.typeTextInHebrew")}</span>
              </label>
              <div className={classes.textareaWrapper}>
                <textarea
                  value={text}
                  onChange={(e) => setText(e.target.value)}
                  className={classes.textarea}
                  rows={3}
                  placeholder={t("pages.avatarOu.typeHereYourText")}
                  dir="rtl"
                />
                <div className={classes.charCounter}>
                  {text.length} {t("pages.avatarOu.characters")}
                </div>
              </div>
            </div>

            {/* Sample Texts */}
            <div>
              <label className={classes.inputLabel}>
                <span>{t("pages.avatarOu.examples")}</span>
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
                    {t("pages.avatarOu.stopSpeaking")}
                  </>
                ) : (
                  <>
                    <Play size={20} />
                    {t("pages.avatarOu.startSpeaking")}
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
                {t("pages.avatarOu.webSpeachApi")}{" "}
                {t("pages.avatarOu.fullHebrewSupport")}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
