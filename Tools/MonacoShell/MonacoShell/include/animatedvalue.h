#pragma once
#include <algorithm>
#include <list>

#undef min
#undef max
namespace aristocrat
{
	template<class T>
	constexpr const T& clamp(const T& v, const T& lo, const T& hi)
	{
		return std::max(lo, std::min(v, hi));
	}


	namespace interpolation
	{
		template<class T> static T linear(T a, T b, double x) { return (T)((b - a) * x) + a; }
		template<class T> static T smoothstep(T a, T b, double x) { return (T)(x*x*(3 - 2 * x)) * (b - a) + a; }

		// use this if cxx14 fails on you
		// template<class T> using func_decl = std::function<T(T,T,double)>;

		template<class T> using func_decl = T(T a, T b, double x);
	};

	template<class T = double>
	class AnimatedValue
	{

	public:

		struct Animation
		{
			Animation(interpolation::func_decl<T> interpolation_func,
                T from_, T to_, double start_, double duration_, int end_state_) 
                : function(interpolation_func)
                , start(start_)
                , duration(duration_)
                , from(from_)
                , to(to_)
                , end_state(end_state_)
			{
			}

			Animation(const Animation& p)
			{
                function = p.function;
				duration = p.duration;
				start = p.start;
				from = p.from;
				to = p.to;
				end_state = p.end_state;
			}
			interpolation::func_decl<T>* function;
			//interpolation::func<T> function;
			T animate(double ntime) { return function(from, to, ntime); }
			T from;
			T to;
			double duration;
			double start;
			int end_state;
		};
	public:
		AnimatedValue() : time(0), _value(0), ntime(0), _state(0)
		{
		}
		AnimatedValue& PushTo(interpolation::func_decl<T> f, T to, double duration, int state = 0)
		{
			animations.clear();
			animations.push_back(Animation(f, _value, to, time - ntime, duration,state));
			return *this;
		}

		AnimatedValue& Push(interpolation::func_decl<T> f, T from, T to, double duration,int state = 0)
		{
			animations.push_back(Animation(f, from, to, time, duration, state));
			return *this;
		}

		void Update(double elapsedTime)
		{
			time += elapsedTime;

			while (!animations.empty())
			{
				Animation& a = animations.front();
				double end_time = a.start + a.duration;
				ntime = (time - a.start) / a.duration;

				if (time >= end_time)
				{
					_value = a.animate(1.0);
					_state = a.end_state;
					animations.pop_front();
					ntime = 0.0;
					if (on_updated != nullptr)
					{
						on_updated(_value);
					}
					if(on_complete != nullptr)
					{
						on_complete(_state);
					}
				}
				else
				{
					if(on_updated != nullptr)
					{
						on_updated(_value);
					}
					_value = a.animate(ntime);
					return;
				}
			};
		};

		bool IsCompleted()
		{
			return animations.empty();
		}
		AnimatedValue& OnComplete(std::function<void(int)> method)
		{
			on_complete = method;
			return *this;
		}
		AnimatedValue& OnUpdated(std::function<void(T)> method)
		{
			on_updated = method;
			return *this;
		}
		void Clear(T v)
		{
			animations.clear();
			_value = v;
			_state = 0;
		}

		int GetState() { return _state;}
		T GetValue() { return _value; }
		void SetValue(T v) { Clear(v); }

	private:
		std::list<Animation> animations;
		std::function<void(int)> on_complete;
		std::function<void(T)> on_updated;
		double ntime;
		double time;
		T _value;
		int _state;
	};
}