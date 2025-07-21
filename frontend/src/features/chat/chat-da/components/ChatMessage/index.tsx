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
    >
      {message.text}
    </div>
  );
};
