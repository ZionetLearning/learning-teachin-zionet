import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { type ImageMessage as ImageMessageType } from "../../../types";
import { useStyles } from "./style";

interface ImageMessageProps {
  message: ImageMessageType;
}

const ImageMessage: React.FC<ImageMessageProps> = ({ message }) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [isLoading, setIsLoading] = useState(true);
  const [hasError, setHasError] = useState(false);

  const handleImageLoad = () => {
    setIsLoading(false);
  };

  const handleImageError = () => {
    setIsLoading(false);
    setHasError(true);
  };

  if (hasError) {
    return (
      <div className={classes.container}>
        <div className={classes.errorState}>
          <div className={classes.errorIcon}>📷</div>
          <div className={classes.errorText}>{t('pages.chatOu.failedToLoadImage')}</div>
          <div className={classes.errorSubtext}>
            {message.content.alt || t('pages.chatOu.imageUnavailable')}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={classes.container}>
      {isLoading && (
        <div className={classes.loadingState}>
          <div className={classes.loadingSpinner}></div>
          <div className={classes.loadingText}>{t('pages.chatOu.loadingImage')}</div>
        </div>
      )}

      <img
        src={message.content.url}
        alt={message.content.alt}
        className={`${classes.image} ${isLoading ? classes.imageHidden : classes.imageVisible}`}
        onLoad={handleImageLoad}
        onError={handleImageError}
      />

      {message.content.caption && !isLoading && !hasError && (
        <div className={classes.caption}>{message.content.caption}</div>
      )}
    </div>
  );
};

export { ImageMessage };
