import type { Message } from "../../../../../types";

import useStyles from "./style";

export const ChatMessage = ({ message }: { message: Message }) => {
  const classes = useStyles();

  return (
    <div
      key={message.id}
      className={`${classes.message} ${
        message.sender === "user" ? classes.userMessage : classes.botMessage
      }`}
      data-testid={
        message.sender === "user"
          ? "chat-da-msg-user"
          : message.isComplete
            ? "chat-da-msg-bot-complete"
            : "chat-da-msg-bot-streaming"
      }
    >
      {message.text}
    </div>
  );
};
