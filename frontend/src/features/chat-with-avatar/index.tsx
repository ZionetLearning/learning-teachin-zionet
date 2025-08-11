import { useAvatarSpeech } from "@/hooks";
import { ChatOpenAI, AvatarView } from "@/components";
import { lipsArray } from "@/assets/lips";

export const ChatWithAvatar = () => {
  const { currentVisemeSrc, speak } = useAvatarSpeech(lipsArray);

  return (
    <ChatOpenAI
      onAssistantText={(text: string) => speak(text)}
      headerAvatar={<AvatarView currentVisemeSrc={currentVisemeSrc} />}
    />
  );
};
