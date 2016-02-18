package %NAMESPACE%;

import java.util.List;

@SuppressWarnings("javadoc")
public interface IFormat
{
    void setIndentation(int indentation);
    int getIndentation();
    String getNewLine();
    String getTabulations();
    void formatHeader(StringBuilder builder, String title);
    void formatFooter(StringBuilder builder);

    <T> void formatField(StringBuilder bd, String title, T field);
    <T> void formatField(StringBuilder bd, String title, List<T> field);

}