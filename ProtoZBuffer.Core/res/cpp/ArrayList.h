#pragma once
#include <memory>
#include <utility>
#include <vector>

%NAMESPACE_BEGIN%

template <class T>
class ArrayList
{
public:

    T* get(std::size_t index) const
    {
        return index < size() ? m_map[index].get() : nullptr;
    }

    void set(std::size_t index, std::unique_ptr<T>&& value)
    {
        // pad with nullptrs so that m_map.size() >= index + 1 
        if (index >= m_map.size())
            m_map.resize(index + 1 - m_map.size());
        m_map[index] = std::move(value);
    }

    std::size_t size() const
    {
        return m_map.size();
    }

    void clear()
    {
        m_map.clear();
    }

private:
    std::vector<std::unique_ptr<T>> m_map;
};

%NAMESPACE_END%
