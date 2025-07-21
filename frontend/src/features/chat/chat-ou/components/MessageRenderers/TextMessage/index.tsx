import React from "react";
import { type TextMessage as TextMessageType } from "../../../types/Message";
import { useStyles } from "./style";

interface TextMessageProps {
  message: TextMessageType;
}

const TextMessage: React.FC<TextMessageProps> = ({ message }) => {
  const classes = useStyles();

  // Basic markdown support for common patterns
  const renderMarkdown = (text: string) => {
    // Convert **bold** to <strong>
    let processed = text.replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>");

    // Convert *italic* to <em>
    processed = processed.replace(/\*(.*?)\*/g, "<em>$1</em>");

    // Convert `code` to <code>
    processed = processed.replace(/`(.*?)`/g, "<code>$1</code>");

    // Convert line breaks to <br>
    processed = processed.replace(/\n/g, "<br>");

    return processed;
  };

  return (
    <div className={classes.container}>
      <div
        className={classes.content}
        dangerouslySetInnerHTML={{
          __html: renderMarkdown(message.content),
        }}
      />
    </div>
  );
};

export { TextMessage };
