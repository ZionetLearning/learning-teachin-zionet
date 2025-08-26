import { useTranslation } from "react-i18next";
import { SendIcon } from "./icons";
import useStyles from "./style";

interface InputProps {
  input: string;
  setInput: (value: string) => void;
  sendMessage: (message: string) => void;
  disabled: boolean;
}

export const ChatInput = ({
  input,
  setInput,
  sendMessage,
  disabled,
}: InputProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  return (
    <footer
      className={classes.inputWrapper}
      data-testid="chat-da-input-wrapper"
    >
      <input
        id="chat-input"
        data-testid="chat-da-input"
        className={classes.input}
        value={input}
        onChange={(e) => setInput(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "Enter" && !disabled && input.trim()) {
            sendMessage(input.trim());
            setInput("");
          }
        }}
        autoComplete="off"
        placeholder={t("pages.chatDa.typeMessage")}
        disabled={disabled}
      />
      <SendIcon
        width={40}
        height={40}
        className={classes.sendButton}
        data-testid="chat-da-send"
        fill={disabled ? "#ccc" : "currentColor"}
        stroke={disabled ? "#ccc" : "currentColor"}
        onClick={() => {
          if (!disabled && input.trim()) {
            sendMessage(input.trim());
          }
          setInput("");
        }}
      />
    </footer>
  );
};
