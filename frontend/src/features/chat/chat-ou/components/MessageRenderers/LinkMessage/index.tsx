import React from "react";
import { useStyles } from "./style";

interface LinkMessageProps {
  title: string;
  description?: string;
  url: string;
  icon?: string;
}

const LinkMessage: React.FC<LinkMessageProps> = ({
  title,
  description,
  url,
  icon,
}) => {
  const classes = useStyles();

  const handleClick = () => {
    // For demo purposes, we'll use window.location for navigation
    // In a real app with react-router-dom, this would use navigate()
    if (url.startsWith("/")) {
      // Internal navigation
      window.location.href = url;
    } else {
      // External link
      window.open(url, "_blank", "noopener,noreferrer");
    }
  };

  return (
    <div className={classes.container} onClick={handleClick}>
      <div className={classes.content}>
        {icon && <div className={classes.icon}>{icon}</div>}
        <div className={classes.textContent}>
          <div className={classes.title}>{title}</div>
          {description && (
            <div className={classes.description}>{description}</div>
          )}
        </div>
        <div className={classes.arrow}>→</div>
      </div>
    </div>
  );
};

export { LinkMessage };
