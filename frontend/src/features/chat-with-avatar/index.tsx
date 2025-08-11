import { useAvatarSpeech } from "@/hooks";
import { ChatOpenAI, AvatarView } from "@/components";
import { lipsArray } from "@/assets/lips";

export const ChatWithAvatar = () => {
  const { currentVisemeSrc, speak } = useAvatarSpeech(lipsArray);

  return (
    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 24 }}>
      <AvatarView currentVisemeSrc={currentVisemeSrc} />
      {/* Pass `speak` so chat can trigger the avatar when AI replies */}
      <ChatOpenAI onAssistantText={(text: string) => speak(text)} />
    </div>
  );
};
