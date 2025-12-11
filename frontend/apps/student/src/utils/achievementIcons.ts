import EmojiEventsIcon from "@mui/icons-material/EmojiEvents";
import StarsIcon from "@mui/icons-material/Stars";
import WorkspacePremiumIcon from "@mui/icons-material/WorkspacePremium";
import KeyboardIcon from "@mui/icons-material/Keyboard";
import RecordVoiceOverIcon from "@mui/icons-material/RecordVoiceOver";
import ReorderIcon from "@mui/icons-material/Reorder";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import EmojiSymbolsIcon from "@mui/icons-material/EmojiSymbols";
import SportsEsportsIcon from "@mui/icons-material/SportsEsports";
import type { SvgIconComponent } from "@mui/icons-material";

export const getAchievementIcon = (key: string): SvgIconComponent => {
  const normalizedKey = key?.toLowerCase?.() ?? "";

  const icons: Record<string, SvgIconComponent> = {
    word_cards_first: EmojiEventsIcon,
    word_cards_3: StarsIcon,
    word_cards_5: WorkspacePremiumIcon,
    word_cards_10: WorkspacePremiumIcon,
    typing_first: KeyboardIcon,
    typing_3: KeyboardIcon,
    typing_5: KeyboardIcon,
    speaking_first: RecordVoiceOverIcon,
    speaking_3: RecordVoiceOverIcon,
    speaking_5: RecordVoiceOverIcon,
    word_order_first: ReorderIcon,
    word_order_3: ReorderIcon,
    word_order_5: ReorderIcon,
    mistakes_first: ErrorOutlineIcon,
    mistakes_3: ErrorOutlineIcon,
    mistakes_5: ErrorOutlineIcon,
    challenge_first: EmojiSymbolsIcon,
    challenge_3: EmojiSymbolsIcon,
    challenge_5: EmojiSymbolsIcon,
  };

  if (icons[normalizedKey]) {
    return icons[normalizedKey];
  }

  const featureIcons: Record<string, SvgIconComponent> = {
    wordcards: EmojiEventsIcon,
    typingpractice: KeyboardIcon,
    speakingpractice: RecordVoiceOverIcon,
    wordorder: ReorderIcon,
    practicemistakes: ErrorOutlineIcon,
    wordcardschallenge: EmojiSymbolsIcon,
    gamepractice: SportsEsportsIcon,
    games: SportsEsportsIcon,
  };

  if (featureIcons[normalizedKey]) {
    return featureIcons[normalizedKey];
  }

  return EmojiEventsIcon;
};
